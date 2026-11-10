#!/usr/bin/env python3
"""Back up or restore the Tutoring PostgreSQL schema and private Storage objects.
Requires pg_dump/pg_restore, SUPABASE_URL, SUPABASE_SERVICE_ROLE_KEY,
PG_BACKUP_URL (libpq URI or key=value), and optionally SUPABASE_BUCKET.
Run on a trusted machine, never in a Vercel function.
"""
import argparse
import hashlib
import json
import os
from pathlib import Path
import subprocess
import sys
import tarfile
import tempfile
from urllib.error import HTTPError
from urllib.parse import quote
from urllib.request import Request, urlopen

BUCKET = os.environ.get('SUPABASE_BUCKET', 'tutoring-private')
BASE = os.environ.get('SUPABASE_URL', '').rstrip('/') + '/storage/v1'
TOKEN = os.environ.get('SUPABASE_SERVICE_ROLE_KEY', '')
DATABASE = os.environ.get('PG_BACKUP_URL', '')


def call(method, path, data=None, content_type='application/json'):
    headers = {'Authorization': 'Bearer ' + TOKEN, 'apikey': TOKEN}
    if data is not None:
        headers['Content-Type'] = content_type
    request = Request(BASE + path, data=data, headers=headers, method=method)
    with urlopen(request, timeout=90) as response:
        return response.read()


def objects():
    offset = 0
    while True:
        body = json.dumps({'prefix': '', 'limit': 100, 'offset': offset}).encode()
        batch = json.loads(call('POST', '/object/list/' + quote(BUCKET), body))
        for entry in batch:
            if entry.get('id') and entry.get('name'):
                yield entry['name']
        if len(batch) < 100:
            break
        offset += len(batch)


def checksum(path):
    digest = hashlib.sha256()
    with path.open('rb') as stream:
        for chunk in iter(lambda: stream.read(1024 * 1024), b''):
            digest.update(chunk)
    return digest.hexdigest()


def archive(destination):
    if destination.exists():
        raise ValueError('O arquivo de saída já existe.')
    with tempfile.TemporaryDirectory() as tmp:
        root = Path(tmp)
        subprocess.run(['pg_dump', '--no-owner', '--no-privileges', '--schema=tutoring',
                        '--format=custom', '--file=' + str(root / 'database.dump'), DATABASE], check=True)
        files = {'database.dump': checksum(root / 'database.dump')}
        folder = root / 'attachments'
        folder.mkdir()
        for name in objects():
            if '/' in name or name in ('', '.', '..'):
                raise ValueError('Nome de objeto inesperado: ' + name)
            target = folder / name
            target.write_bytes(call('GET', '/object/authenticated/' + quote(BUCKET) + '/' + quote(name)))
            files['attachments/' + name] = checksum(target)
        (root / 'manifest.json').write_text(json.dumps({'bucket': BUCKET, 'files': files}, indent=2), encoding='utf-8')
        with tarfile.open(destination, 'w:gz') as bundle:
            for name in files:
                bundle.add(root / name, arcname=name)
            bundle.add(root / 'manifest.json', arcname='manifest.json')
    print('Backup salvo em', destination, 'com', len(files)-1, 'anexos. Guarde-o fora dos serviços.')


def restore(source):
    with tempfile.TemporaryDirectory() as tmp:
        root = Path(tmp)
        with tarfile.open(source, 'r:gz') as bundle:
            for item in bundle.getmembers():
                if not item.isfile() or item.name.startswith('/') or '..' in Path(item.name).parts:
                    raise ValueError('Entrada inválida no arquivo de backup')
                if item.name != 'manifest.json' and item.name != 'database.dump' and not item.name.startswith('attachments/'):
                    raise ValueError('Entrada inesperada no backup')
            bundle.extractall(root, filter='data')
        manifest = json.loads((root / 'manifest.json').read_text(encoding='utf-8'))
        for name, expected in manifest['files'].items():
            if checksum(root / name) != expected:
                raise ValueError('Integridade inválida: ' + name)
        if list(objects()):
            raise ValueError('O bucket de destino deve estar vazio.')
        # Restoring is intentionally restricted to a freshly prepared staging database.
        subprocess.run(['pg_restore', '--no-owner', '--no-privileges', '--exit-on-error',
                        '--dbname=' + DATABASE, str(root / 'database.dump')], check=True)
        for name in manifest['files']:
            if name.startswith('attachments/'):
                key = name[len('attachments/'):]
                call('POST', '/object/' + quote(BUCKET) + '/' + quote(key),
                     (root / name).read_bytes(), 'application/octet-stream')
        print('Restauração concluída. Confira login, registros e anexos no ambiente de teste.')


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('action', choices=['backup', 'restore'])
    parser.add_argument('file', type=Path)
    args = parser.parse_args()
    if not BASE.startswith('https://') or not TOKEN or not DATABASE:
        parser.error('Configure PG_BACKUP_URL (string libpq), SUPABASE_URL e SUPABASE_SERVICE_ROLE_KEY.')
    if args.action == 'backup':
        archive(args.file)
    else:
        if os.environ.get('CONFIRM_EMPTY_STAGING') != 'yes':
            parser.error('Restaure somente em banco e bucket vazios de teste; defina CONFIRM_EMPTY_STAGING=yes.')
        restore(args.file)


if __name__ == '__main__':
    try:
        main()
    except (HTTPError, subprocess.CalledProcessError, ValueError, OSError) as exc:
        print('Falha:', exc, file=sys.stderr)
        sys.exit(1)
