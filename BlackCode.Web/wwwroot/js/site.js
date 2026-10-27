const toggle=document.querySelector('[data-search-toggle]');const search=document.querySelector('[data-search]');
toggle?.addEventListener('click',()=>{search.hidden=!search.hidden;toggle.setAttribute('aria-expanded',String(!search.hidden));if(!search.hidden)search.querySelector('input').focus();});
document.addEventListener('keydown',e=>{if(e.key==='Escape'&&search){search.hidden=true;toggle?.setAttribute('aria-expanded','false');}});
document.querySelectorAll('form[data-confirm]').forEach(form=>form.addEventListener('submit',event=>{if(!confirm(form.dataset.confirm))event.preventDefault();}));
document.querySelectorAll('[data-submit-once]').forEach(button=>button.form.addEventListener('submit',()=>{if(button.form.checkValidity()){button.disabled=true;button.textContent='Procesando…';}}));
