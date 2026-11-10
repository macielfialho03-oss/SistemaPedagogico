using System.Net.Http.Headers;
using SistemaPedagogico.Infrastructure.Storage;

namespace SistemaPedagogico.Web.Portal;

public interface IAttachmentStorage {
 Task PutAsync(string key, byte[] data, string contentType);
 Task<byte[]?> GetAsync(string key);
 Task DeleteAsync(string key);
}

public sealed class LocalAttachmentStorage(StoragePaths paths):IAttachmentStorage {
 string DirectoryPath => Path.Combine(paths.DataDirectory,"Arquivos");
 public async Task PutAsync(string key,byte[] data,string contentType){Directory.CreateDirectory(DirectoryPath);await File.WriteAllBytesAsync(Path.Combine(DirectoryPath,key),data);}
 public async Task<byte[]?> GetAsync(string key){var path=Path.Combine(DirectoryPath,key);return File.Exists(path)?await File.ReadAllBytesAsync(path):null;}
 public Task DeleteAsync(string key){File.Delete(Path.Combine(DirectoryPath,key));return Task.CompletedTask;}
}

// The service key never reaches the browser. Each download is authorized by PortalController.
public sealed class SupabaseAttachmentStorage(HttpClient http,IConfiguration config):IAttachmentStorage {
 string Endpoint => (config["SUPABASE_URL"]??throw new InvalidOperationException("SUPABASE_URL ausente")).TrimEnd('/')+"/storage/v1/object/";
 string Bucket => config["SUPABASE_BUCKET"]??"tutoring-private";
 string Token => config["SUPABASE_SERVICE_ROLE_KEY"]??throw new InvalidOperationException("SUPABASE_SERVICE_ROLE_KEY ausente");
 HttpRequestMessage Request(HttpMethod method,string key,bool authenticated=false){
  var uri=Endpoint+(authenticated?"authenticated/":"")+Uri.EscapeDataString(Bucket)+"/"+Uri.EscapeDataString(key);
  var request=new HttpRequestMessage(method,uri);request.Headers.Authorization=new AuthenticationHeaderValue("Bearer",Token);request.Headers.Add("apikey",Token);return request;
 }
 public async Task PutAsync(string key,byte[] data,string contentType){using var request=Request(HttpMethod.Post,key);request.Content=new ByteArrayContent(data);request.Content.Headers.ContentType=new MediaTypeHeaderValue(contentType);using var response=await http.SendAsync(request);response.EnsureSuccessStatusCode();}
 public async Task<byte[]?> GetAsync(string key){using var request=Request(HttpMethod.Get,key,true);using var response=await http.SendAsync(request);if(response.StatusCode==System.Net.HttpStatusCode.NotFound)return null;response.EnsureSuccessStatusCode();return await response.Content.ReadAsByteArrayAsync();}
 public async Task DeleteAsync(string key){using var request=Request(HttpMethod.Delete,key);using var response=await http.SendAsync(request);response.EnsureSuccessStatusCode();}
}
