(() => {
'use strict';
const data=JSON.parse(document.getElementById('portal-data')?.textContent || '{"students":[],"teachers":[],"enrollments":[]}');
const optionsCache=new WeakMap();
function remember(select){if(!optionsCache.has(select))optionsCache.set(select,[...select.options].map(x=>x.cloneNode(true)));return optionsCache.get(select);}
function filter(select,predicate){if(!select)return;const old=select.value;select.replaceChildren(...remember(select).filter(x=>!x.value||predicate(x)).map(x=>x.cloneNode(true)));if([...select.options].some(x=>x.value===old))select.value=old;select.dispatchEvent(new Event('optionschanged'));}
// Restore values before dependent fields are filtered.
document.querySelectorAll('select[data-current]').forEach(select=>{const value=select.dataset.current;if([...select.options].some(option=>option.value===value))select.value=value;else if(value){select.add(new Option(value,value,true,true));}});
// Brazilian names: spaces and capitalization, without guessing accents.
const fullName=value=>value.trim().replace(/\s+/gu,' ').toLocaleLowerCase('pt-BR').replace(/(^|[\s'’\-])(\p{L})/gu,(_,prefix,char)=>prefix+char.toLocaleUpperCase('pt-BR'));
document.querySelectorAll('[data-fullname]').forEach(input=>{const validate=()=>{const value=input.value.trim();input.setCustomValidity(value&&(!/^[\p{L}\p{M}]+(?:['’\- ][\p{L}\p{M}]+)*$/u.test(value.replace(/\s+/gu,' '))||value.split(/\s+/u).length<2)?'Informe nome e sobrenome usando apenas letras.':'');};input.addEventListener('input',validate);input.addEventListener('blur',()=>{input.value=fullName(input.value);validate();});validate();});
// Dependent student / teacher / subject selection on pedagogical forms.
document.querySelectorAll('form').forEach(form=>{
 const kind=new URL(form.action,location.href).searchParams.get('kind');
 if(!['lesson','material','progress','grade','activity'].includes(kind))return;
 const student=form.elements.StudentId,teacher=form.elements.TeacherId,subject=form.elements.SubjectId;
 const service=form.elements.ServiceType;
 const recipients=()=>{const checked=[...form.querySelectorAll('[name="StudentIds"]:checked')].map(x=>Number(x.value));return checked.length?checked:[data.role==='Aluno'?data.entity:Number(student?.value||0)].filter(Boolean);};
 const update=(teacherChanged=false)=>{
  const ids=recipients();
  if(teacher?.tagName==='SELECT'&&!teacherChanged)filter(teacher,o=>ids.length>0&&ids.every(s=>data.enrollments.some(e=>e.student===s&&e.teacher===Number(o.value))));
  const tid=data.role==='Professor'?data.entity:Number(teacher?.value||0);const selectedTeacher=data.teachers.find(t=>t.id===tid);
  if(subject)filter(subject,o=>ids.length>0&&ids.every(s=>data.students.find(x=>x.id===s)?.subjects.includes(Number(o.value)))&&(!tid||selectedTeacher?.subjects.includes(Number(o.value))));
  if(service){const s=data.students.find(x=>x.id===ids[0]);filter(service,o=>!s||s.service==='Ambos'||o.value===s.service);showService();}
  if(form.hasAttribute('data-lesson'))loadSlots(form);
 };
 function showService(){form.querySelectorAll('[data-service]').forEach(label=>{const visible=label.dataset.service===service.value;label.hidden=!visible;label.querySelectorAll('input,textarea,select').forEach(x=>x.disabled=!visible);});}
 student?.addEventListener('change',()=>update());teacher?.addEventListener('change',()=>update(true));form.querySelectorAll('[name="StudentIds"]').forEach(x=>x.addEventListener('change',()=>{form.querySelectorAll('[name="StudentIds"]').forEach(y=>y.setCustomValidity(''));update();}));service?.addEventListener('change',showService);
 if(form.hasAttribute('data-lesson')){
  ['LessonDate','Minutes','SubjectId'].forEach(name=>form.elements[name]?.addEventListener('change',()=>loadSlots(form)));
  const online=()=>{const visible=form.elements.Mode.value==='Online';form.querySelector('[data-online]').hidden=!visible;form.elements.OnlineUrl.disabled=!visible;};form.elements.Mode.addEventListener('change',online);online();
 }
 update();
});
async function loadSlots(form){
 const e=form.elements,target=e.Start,note=form.querySelector('[data-slots-note]');
 const student=data.role==='Aluno'?data.entity:e.StudentId?.value,teacher=data.role==='Professor'?data.entity:e.TeacherId?.value;
 const params=new URLSearchParams({studentId:student||'',teacherId:teacher||'',subjectId:e.SubjectId.value,date:e.LessonDate.value,minutes:e.Minutes.value,lessonId:e.Id.value});
 const request=Symbol();form.slotRequest=request;target.replaceChildren(new Option('Escolha aluno, professor, matéria e dia',''));note.textContent='';
 if(!student||!teacher||!e.SubjectId.value||!e.LessonDate.value)return;
 target.disabled=true;note.textContent='Consultando horários…';
 try{const result=await fetch('/portal/horarios?'+params,{headers:{Accept:'application/json'}});if(!result.ok)throw new Error();const slots=await result.json();if(form.slotRequest!==request)return;target.replaceChildren(new Option(slots.length?'Selecione o horário':'Nenhum horário disponível',''),...slots.map(x=>new Option(x.slice(11,16),x)));if(target.dataset.selected&&slots.includes(target.dataset.selected))target.value=target.dataset.selected;note.textContent=slots.length?'Horários considerando a duração e os agendamentos existentes.':'Escolha outro dia, professor ou duração.';}
 catch{if(form.slotRequest===request)note.textContent='Não foi possível consultar. Tente selecionar o dia novamente.';}
 finally{if(form.slotRequest===request)target.disabled=false;}
}
// Search choices without an external dependency.
document.querySelectorAll('select').forEach(select=>{
 if(!['StudentId','TeacherId','SubjectId','EntityId'].includes(select.name)||select.type==='hidden')return;
 const search=document.createElement('input');search.type='search';search.placeholder='Buscar pelo nome…';search.className='select-search';search.setAttribute('aria-label','Buscar opções de '+select.name);select.before(search);
 const apply=()=>{const query=search.value.toLocaleLowerCase('pt-BR');[...select.options].forEach(o=>o.hidden=!!o.value&&!o.textContent.toLocaleLowerCase('pt-BR').includes(query));};search.addEventListener('input',apply);select.addEventListener('optionschanged',()=>{search.value='';apply();});
});
document.querySelectorAll('[data-choice-search]').forEach(input=>input.addEventListener('input',()=>input.closest('fieldset').querySelectorAll('.choice-option').forEach(label=>label.hidden=!label.textContent.toLocaleLowerCase('pt-BR').includes(input.value.toLocaleLowerCase('pt-BR')))));
// Billing exists only on initial student registration.
document.querySelectorAll('[data-billing]').forEach(group=>{
 const months=group.querySelector('[name="BillingMonths"]'),amount=group.querySelector('[name="MonthlyAmount"]'),due=group.querySelector('[name="FirstDue"]'),preview=group.parentElement.querySelector('[data-billing-preview]');
 const update=()=>{const enabled=Number(months.value)>0;[amount,due].forEach(x=>{x.disabled=!enabled;x.required=enabled;});preview.textContent=enabled?`${months.value} mensalidades de ${Number(amount.value||0).toLocaleString('pt-BR',{style:'currency',currency:'BRL'})}, a partir do primeiro vencimento.`:'Sem geração de mensalidades.';};[months,amount,due].forEach(x=>x.addEventListener('input',update));update();
});
// Accounts choose their associated person by name.
document.querySelectorAll('[data-user]').forEach(form=>{
 const e=form.elements,entity=e.EntityId;remember(entity);
 function profile(fill=false){const admin=e.Role.value==='Admin';form.querySelector('[data-account-entity]').hidden=admin;entity.disabled=admin;entity.required=!admin;e.Name.readOnly=!admin;filter(entity,o=>o.dataset.role===e.Role.value);if(!admin&&fill)person();}
 function person(){const option=entity.selectedOptions[0];if(option?.value){e.Name.value=option.textContent;e.Name.dispatchEvent(new Event('input'));if(option.dataset.email)e.Email.value=option.dataset.email;}}
 e.Role.addEventListener('change',()=>{entity.value='';e.Name.value='';profile(true);});entity.addEventListener('change',person);profile(false);
 form.querySelector('[data-show-password]')?.addEventListener('click',event=>{e.Password.type=e.Password.type==='password'?'text':'password';event.target.textContent=e.Password.type==='password'?'Mostrar senha':'Ocultar senha';});
 form.querySelector('[data-generate-password]')?.addEventListener('click',()=>{const bytes=crypto.getRandomValues(new Uint8Array(16));const alphabet='ABCDEFGHJKLMNPQRSTUVWXYZabcdefghijkmnopqrstuvwxyz23456789!@#$';e.Password.value=[...bytes].map(x=>alphabet[x%alphabet.length]).join('');e.Password.type='text';form.querySelector('[data-show-password]').textContent='Ocultar senha';});
});
// Consistent modal editors keep forms outside the table layout.
document.querySelectorAll('td details,article>details').forEach(details=>{
 const summary=details.querySelector(':scope > summary');const button=document.createElement('button');button.type='button';button.className='btn '+(summary.classList.contains('danger')?'danger':'subtle');button.textContent=summary.textContent;
 const dialog=document.createElement('dialog');dialog.className='editor-dialog';const header=document.createElement('div');header.className='panel-head';const title=document.createElement('h2');title.textContent=summary.textContent;const close=document.createElement('button');close.type='button';close.className='btn subtle';close.textContent='Fechar';header.append(title,close);dialog.append(header);[...details.children].filter(x=>x!==summary).forEach(x=>dialog.append(x));details.replaceWith(button);document.body.append(dialog);button.addEventListener('click',()=>dialog.showModal());close.addEventListener('click',()=>dialog.close());dialog.addEventListener('click',event=>{if(event.target===dialog){const rect=dialog.getBoundingClientRect();if(event.clientX<rect.left||event.clientX>rect.right||event.clientY<rect.top||event.clientY>rect.bottom)dialog.close();}});
});
// Friendly file selection feedback.
document.querySelectorAll('input[type="file"]').forEach(input=>{const help=document.createElement('small');input.after(help);const clear=document.createElement('button');clear.type='button';clear.className='btn subtle';clear.textContent='Remover arquivo';clear.hidden=true;input.after(clear);input.addEventListener('change',()=>{const file=input.files[0];input.setCustomValidity(file&&file.size>10*1024*1024?'O arquivo deve ter no máximo 10 MB.':'');help.textContent=file?`${file.name} · ${(file.size/1024/1024).toFixed(2)} MB`:'Nenhum arquivo selecionado.';clear.hidden=!file;});clear.addEventListener('click',()=>{input.value='';input.dispatchEvent(new Event('change'));});});
document.querySelector('[data-table-filter]')?.addEventListener('change',event=>document.querySelectorAll('tr[data-status]').forEach(row=>row.hidden=!!event.target.value&&row.dataset.status!==event.target.value));
document.querySelectorAll('form').forEach(form=>form.addEventListener('submit',event=>{
 if(form.dataset.confirm&&!confirm(form.dataset.confirm)){event.preventDefault();return;}
 const recipients=[...form.querySelectorAll('[name="StudentIds"]')];if(recipients.length&&!recipients.some(x=>x.checked)){event.preventDefault();recipients[0].setCustomValidity('Selecione pelo menos um aluno.');recipients[0].reportValidity();return;}if(!form.checkValidity()){event.preventDefault();return;}
 const button=event.submitter||form.querySelector('button:not([type]),button[type="submit"]');if(button){button.disabled=true;button.textContent='Aguarde…';}
}));
function openRegistration(){const id=location.hash.slice(1);let editor=document.getElementById(id);if(!editor&&/^(aluno|professor)-\d+$/.test(id)){editor=document.getElementById('novo-acesso');const form=editor?.querySelector('form');if(form){const [role,entity]=id.split('-');form.elements.Role.value=role==='aluno'?'Aluno':'Professor';form.elements.Role.dispatchEvent(new Event('change'));form.elements.EntityId.value=entity;form.elements.EntityId.dispatchEvent(new Event('change'));}}if(editor?.tagName==='DETAILS'){editor.open=true;editor.scrollIntoView({block:'start',behavior:'smooth'});editor.querySelector('input:not([type="hidden"]),select')?.focus({preventScroll:true});}}
openRegistration();window.addEventListener('hashchange',openRegistration);
})();
