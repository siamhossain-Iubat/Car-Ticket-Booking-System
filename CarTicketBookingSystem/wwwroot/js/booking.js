document.addEventListener('DOMContentLoaded', () => {
  const $ = id => document.getElementById(id);
  const routeRows = [...document.querySelectorAll('.route-row')];
  const seats = [...document.querySelectorAll('.seat')];
  const routeInput = $('routeInput'), routeIdInput = $('selectedRouteId');
  const dateInput = $('travelDate'), seatInput = $('seatInput');
  let routeId = '', fare = 0, selectedSeat = '', requestNo = 0;

  if (dateInput) {
    const today = new Date();
    const localToday = new Date(today.getTime() - today.getTimezoneOffset() * 60000).toISOString().slice(0,10);
    dateInput.min = localToday;
    if (!dateInput.value) dateInput.value = localToday;
  }
  const msg = text => { if ($('passengerMessage')) $('passengerMessage').textContent = text; };
  const csrf = () => document.querySelector('input[name="__RequestVerificationToken"]')?.value || '';
  function clearSelected() {
    selectedSeat = '';
    seats.forEach(s => s.classList.remove('selected-seat','selected'));
    if (seatInput) seatInput.value = '';
    $('selectedSeat') && ($('selectedSeat').textContent = 'None');
    $('paymentSeat') && ($('paymentSeat').textContent = 'None');
  }
  function setSeatBooked(button) {
    button.classList.remove('available-seat','selected-seat','selected');
    button.classList.add('booked'); button.disabled = true;
  }
  async function loadSeats() {
    const myReq = ++requestNo;
    clearSelected();
    seats.forEach(s => { s.classList.remove('booked','selected-seat','selected'); s.classList.add('available-seat'); s.disabled = true; });
    if (!routeId || !dateInput?.value) { msg('Select travel date and route to see seats.'); return; }
    msg('Loading seats...');
    try {
      const res = await fetch(`/Booking/GetBookedSeats?routeId=${encodeURIComponent(routeId)}&travelDate=${encodeURIComponent(dateInput.value)}`, { cache:'no-store' });
      if (!res.ok) throw new Error(`Seat request failed (${res.status})`);
      const booked = await res.json(); if (myReq !== requestNo) return;
      const bookedSet = new Set(booked.map(x => String(x).trim().toUpperCase()));
      seats.forEach(s => { const name=(s.dataset.seat||'').trim().toUpperCase(); if (bookedSet.has(name)) setSeatBooked(s); else { s.disabled=false; s.classList.remove('booked'); s.classList.add('available-seat'); } });
      msg('Seats updated for selected route and travel date.');
    } catch(e) { if (myReq !== requestNo) return; console.error(e); msg('Could not load seats. Check server/database and try again.'); }
  }
  function chooseRoute(row) {
    routeRows.forEach(r=>r.classList.remove('selected-route')); row.classList.add('selected-route');
    routeId = row.dataset.routeId || ''; fare = Number(row.dataset.fare)||0;
    if(routeInput) routeInput.value=row.dataset.route||''; if(routeIdInput) routeIdInput.value=routeId;
    if($('paymentRoute')) $('paymentRoute').textContent=row.dataset.route||'—';
    if($('paymentFare')) $('paymentFare').textContent=`${fare} BDT`;
    loadSeats();
  }
  routeRows.forEach(row=>{ row.addEventListener('click',()=>chooseRoute(row)); row.addEventListener('keydown',e=>{if(e.key==='Enter'||e.key===' '){e.preventDefault();chooseRoute(row);}}); });
  routeInput?.addEventListener('change',()=>{ const text=routeInput.value.trim().toLowerCase(); const row=routeRows.find(r=>(r.dataset.route||'').trim().toLowerCase()===text); if(row) chooseRoute(row); else {routeId='';fare=0;routeIdInput.value='';loadSeats();} });
  dateInput?.addEventListener('change',loadSeats);
  function chooseSeat(s) {
    if(!routeId || !dateInput.value){msg('Select route and travel date first.');return;}
    if(s.disabled || s.classList.contains('booked')){msg('This seat is already booked.');return;}
    seats.forEach(x=>x.classList.remove('selected-seat','selected')); selectedSeat=(s.dataset.seat||'').toUpperCase();
    s.classList.add('selected-seat','selected'); seatInput.value=selectedSeat;
    $('selectedSeat') && ($('selectedSeat').textContent=selectedSeat); $('paymentSeat') && ($('paymentSeat').textContent=selectedSeat); msg('Seat selected. Enter passenger details.');
  }
  seats.forEach(s=>s.addEventListener('click',()=>chooseSeat(s)));
  seatInput?.addEventListener('change',()=>{const val=seatInput.value.trim().toUpperCase(); const s=seats.find(x=>(x.dataset.seat||'').toUpperCase()===val); if(!s){selectedSeat='';msg('Invalid seat number.');return;} chooseSeat(s);});
  document.querySelectorAll('.menu-item').forEach(b=>b.addEventListener('click',()=>{document.querySelectorAll('.menu-item').forEach(x=>x.classList.remove('active'));b.classList.add('active');const target=b.dataset.target;if(target==='exit'){alert('Thank you for using Car Ticket Booking System!');return;}$(target)?.scrollIntoView({behavior:'smooth',block:'center'});if(target==='history') loadToday();}));

  function localDateISO(){const d=new Date();d.setMinutes(d.getMinutes()-d.getTimezoneOffset());return d.toISOString().slice(0,10);}
  async function loadToday(){const tbody=$('bookingHistory');if(!tbody)return;tbody.innerHTML='<tr><td colspan="6">Loading today\'s bookings...</td></tr>';try{const res=await fetch('/Booking/GetBookingsByDate?date='+encodeURIComponent(localDateISO()),{cache:'no-store'});if(!res.ok)throw Error('Unable to load bookings');const data=await res.json();tbody.innerHTML='';if(!data.length){tbody.innerHTML='<tr><td colspan="6" class="empty-row">No bookings made today.</td></tr>';return;}data.forEach((b,i)=>{const tr=document.createElement('tr');[i+1,b.ticketId,b.route,b.seatNumber,b.travelDate,b.status].forEach(v=>{const td=document.createElement('td');td.textContent=v??'';tr.appendChild(td);});tbody.appendChild(tr);});}catch(e){console.error(e);tbody.innerHTML='<tr><td colspan="6">Could not load today\'s bookings.</td></tr>';}}

  $('confirmBooking')?.addEventListener('click',async()=>{
    const name=$('passengerName')?.value.trim()||'', phone=$('phone')?.value.trim()||'', age=$('age')?.value||'', gender=$('gender')?.value||'';
    const payment=document.querySelector('input[name="payment"]:checked')?.value||'';
    if(!routeId||!dateInput.value){alert('Please select route and travel date.');return;}
    if(!selectedSeat||!seatInput.value.trim()){alert('Please select an available seat.');return;}
    if(!name||!phone||!age||!gender){alert('Please complete passenger details.');return;}
    if(!/^01[3-9]\d{8}$/.test(phone)){alert('Enter a valid Bangladesh mobile number.');return;}
    const s=seats.find(x=>(x.dataset.seat||'').toUpperCase()===selectedSeat);if(!s||s.disabled||s.classList.contains('booked')){alert('Seat is unavailable. Refresh seat availability.');await loadSeats();return;}
    const button=$('confirmBooking');button.disabled=true;
    try{
      const response=await fetch('/Booking/CreateCounter',{method:'POST',headers:{'Content-Type':'application/json'},body:JSON.stringify({PassengerName:name,Phone:phone,SeatNumber:selectedSeat,TravelRouteId:Number(routeId),TravelDate:dateInput.value,PaymentMethod:payment,Fare:fare})});
      const result=await response.json().catch(()=>({message:'Unexpected server response.'}));
      if(!response.ok){alert(result.message||'Booking failed.');await loadSeats();return;}
      $('ticketId') && ($('ticketId').textContent=result.ticketId); $('ticketName') && ($('ticketName').textContent=name); $('ticketRoute') && ($('ticketRoute').textContent=routeInput.value); $('ticketSeat') && ($('ticketSeat').textContent=selectedSeat); $('ticketFare') && ($('ticketFare').textContent=`${result.fare} BDT`); $('ticketDate') && ($('ticketDate').textContent=dateInput.value); $('ticketPayment') && ($('ticketPayment').textContent=payment); $('ticketStatus') && ($('ticketStatus').textContent='Booking confirmed and saved.');
      alert(`Booking confirmed! Ticket ID: ${result.ticketId}`); await loadSeats(); await loadToday(); clearSelected();
      ['passengerName','phone','nid','age'].forEach(id=>{if($(id))$(id).value='';});if($('gender'))$('gender').value='';
    }catch(e){console.error(e);alert('Server connection failed. Booking was not confirmed.');}finally{button.disabled=false;}
  });
  $('cancelBooking')?.addEventListener('click',async()=>{const ticketId=$('cancelTicketId')?.value.trim()||'';if(!ticketId){alert('Enter ticket number.');return;}if(!confirm(`Cancel ticket ${ticketId}?`))return;try{const body=new URLSearchParams({ticketId,__RequestVerificationToken:csrf()});const res=await fetch('/Booking/CancelByTicket',{method:'POST',headers:{'Content-Type':'application/x-www-form-urlencoded'},body});const data=await res.json().catch(()=>({message:'Unexpected response'}));$('cancelMessage')&&($('cancelMessage').textContent=data.message||'');if(!res.ok){alert(data.message||'Cancellation failed.');return;}alert(data.message);await loadSeats();await loadToday();}catch(e){console.error(e);alert('Cancellation request failed.');}});
  loadToday();
});
