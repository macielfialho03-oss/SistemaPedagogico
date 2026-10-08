(()=>{
function initPhoneFields(){
function phoneDigits(value){
  let digits=value.replace(/\D/g,'');
  if((digits.length===12||digits.length===13)&&digits.startsWith('55'))digits=digits.slice(2);
  return digits.slice(0,11);
}
function formatPhone(value){
  const digits=phoneDigits(value);
  if(!digits)return '';
  if(digits.length<=2)return '('+digits;
  const local=digits.slice(2),split=local.length>8?5:4;
  return '('+digits.slice(0,2)+') '+local.slice(0,split)+(local.length>split?'-'+local.slice(split):'');
}
document.querySelectorAll('input[data-phone], input[name="Phone"], input[name="GuardianPhone"]').forEach(input=>{
  function update(preserveCaret){
    const old=input.value,caret=input.selectionStart??old.length;
    const count=old.slice(0,caret).replace(/\D/g,'').length;
    // Preserve malformed existing records so editing does not silently change the number.
    const existingDigits=old.replace(/\D/g,'');
    const existingInvalid=/[^0-9 ()+\-]/.test(old)||(existingDigits.length>11&&!((existingDigits.length===12||existingDigits.length===13)&&existingDigits.startsWith('55')));
    if(preserveCaret||!existingInvalid)input.value=formatPhone(old);
    const digits=phoneDigits(input.value);
    input.setCustomValidity((old.length&&!preserveCaret&&existingInvalid)||(digits.length&&!/^[1-9][0-9](?:[2-5][0-9]{7}|9[0-9]{8})$/.test(digits))?'Informe um telefone completo com DDD: (69) 3333-3333 ou (69) 99999-9999.':'');
    if(preserveCaret){
      let position=input.value.length;
      if(caret<old.length){let seen=0;position=0;while(position<input.value.length&&seen<count){if(/\d/.test(input.value[position]))seen++;position++;}}
      input.setSelectionRange(position,position);
    }
  }
  input.type='tel';input.inputMode='tel';input.placeholder='(69) 99999-9999';
  input.pattern='\\([1-9][0-9]\\) (?:[2-5][0-9]{3}|9[0-9]{4})-[0-9]{4}';
  update(false);
  input.addEventListener('input',()=>update(true));
});

}
if(document.readyState==='loading')document.addEventListener('DOMContentLoaded',initPhoneFields);else initPhoneFields();
})();
