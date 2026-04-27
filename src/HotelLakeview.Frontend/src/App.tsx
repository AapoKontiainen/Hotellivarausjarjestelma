import { useMemo, useState } from 'react';
import { format, addDays } from 'date-fns';
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import {
  createBooking,
  createCustomer,
  createRoom,
  createRoomType,
  deleteBooking,
  deleteCustomer,
  deleteRoom,
  deleteRoomType,
  fetchBookings,
  fetchCustomers,
  fetchOccupancy,
  fetchPopularRoomTypes,
  fetchRevenue,
  fetchRooms,
  fetchRoomTypes,
  searchAvailability,
  updateBooking,
  type Booking
} from './api';

type Tab = 'customers' | 'rooms' | 'bookings' | 'availability' | 'reports';

type BookingFormState = {
  customerId: string;
  roomId: string;
  checkInDate: string;
  checkOutDate: string;
  guestCount: string;
  specialRequests: string;
};

const today = new Date();
const defaultCheckIn = format(today, 'yyyy-MM-dd');
const defaultCheckOut = format(addDays(today, 1), 'yyyy-MM-dd');

export default function App() {
  const queryClient = useQueryClient();
  const [activeTab, setActiveTab] = useState<Tab>('customers');
  const [notice, setNotice] = useState<string>('');

  const [customerForm, setCustomerForm] = useState({
    firstName: '',
    lastName: '',
    email: '',
    phone: '',
    notes: ''
  });

  const [roomTypeForm, setRoomTypeForm] = useState({
    name: '',
    description: '',
    baseNightlyRate: '119',
    maxGuests: '2'
  });

  const [roomForm, setRoomForm] = useState({
    roomNumber: '',
    roomTypeId: ''
  });

  const [bookingForm, setBookingForm] = useState<BookingFormState>({
    customerId: '',
    roomId: '',
    checkInDate: defaultCheckIn,
    checkOutDate: defaultCheckOut,
    guestCount: '1',
    specialRequests: ''
  });

  const [editingBookingId, setEditingBookingId] = useState<string | null>(null);

  const [availabilityForm, setAvailabilityForm] = useState({
    checkInDate: defaultCheckIn,
    checkOutDate: defaultCheckOut,
    guestCount: '1',
    roomTypeId: ''
  });
  const [availabilityResults, setAvailabilityResults] = useState<Array<{ roomId: string; roomNumber: string; roomTypeName: string; nightlyRate: number; totalPrice: number }>>([]);

  const [reportRange, setReportRange] = useState({
    fromDate: format(addDays(today, -30), 'yyyy-MM-dd'),
    toDate: format(today, 'yyyy-MM-dd')
  });

  const customersQuery = useQuery({ queryKey: ['customers'], queryFn: fetchCustomers });
  const roomTypesQuery = useQuery({ queryKey: ['room-types'], queryFn: fetchRoomTypes });
  const roomsQuery = useQuery({ queryKey: ['rooms'], queryFn: fetchRooms });
  const bookingsQuery = useQuery({ queryKey: ['bookings'], queryFn: fetchBookings });

  const occupancyQuery = useQuery({
    queryKey: ['analytics', 'occupancy', reportRange.fromDate, reportRange.toDate],
    queryFn: () => fetchOccupancy(reportRange.fromDate, reportRange.toDate)
  });

  const revenueQuery = useQuery({
    queryKey: ['analytics', 'revenue', reportRange.fromDate, reportRange.toDate],
    queryFn: () => fetchRevenue(reportRange.fromDate, reportRange.toDate)
  });

  const popularityQuery = useQuery({
    queryKey: ['analytics', 'popularity', reportRange.fromDate, reportRange.toDate],
    queryFn: () => fetchPopularRoomTypes(reportRange.fromDate, reportRange.toDate)
  });

  const createCustomerMutation = useMutation({
    mutationFn: createCustomer,
    onSuccess: async () => {
      setCustomerForm({ firstName: '', lastName: '', email: '', phone: '', notes: '' });
      setNotice('Asiakas luotu.');
      await queryClient.invalidateQueries({ queryKey: ['customers'] });
    },
    onError: (error: Error) => setNotice(error.message)
  });

  const deleteCustomerMutation = useMutation({
    mutationFn: deleteCustomer,
    onSuccess: async () => {
      setNotice('Asiakas poistettu.');
      await queryClient.invalidateQueries({ queryKey: ['customers'] });
      await queryClient.invalidateQueries({ queryKey: ['bookings'] });
    },
    onError: (error: Error) => setNotice(error.message)
  });

  const createRoomTypeMutation = useMutation({
    mutationFn: createRoomType,
    onSuccess: async () => {
      setRoomTypeForm({ name: '', description: '', baseNightlyRate: '119', maxGuests: '2' });
      setNotice('Huonetyyppi luotu.');
      await queryClient.invalidateQueries({ queryKey: ['room-types'] });
    },
    onError: (error: Error) => setNotice(error.message)
  });

  const deleteRoomTypeMutation = useMutation({
    mutationFn: deleteRoomType,
    onSuccess: async () => {
      setNotice('Huonetyyppi arkistoitu.');
      await queryClient.invalidateQueries({ queryKey: ['room-types'] });
      await queryClient.invalidateQueries({ queryKey: ['rooms'] });
    },
    onError: (error: Error) => setNotice(error.message)
  });

  const createRoomMutation = useMutation({
    mutationFn: createRoom,
    onSuccess: async () => {
      setRoomForm({ roomNumber: '', roomTypeId: '' });
      setNotice('Huone luotu.');
      await queryClient.invalidateQueries({ queryKey: ['rooms'] });
    },
    onError: (error: Error) => setNotice(error.message)
  });

  const deleteRoomMutation = useMutation({
    mutationFn: deleteRoom,
    onSuccess: async () => {
      setNotice('Huone arkistoitu.');
      await queryClient.invalidateQueries({ queryKey: ['rooms'] });
      await queryClient.invalidateQueries({ queryKey: ['bookings'] });
    },
    onError: (error: Error) => setNotice(error.message)
  });

  const createBookingMutation = useMutation({
    mutationFn: createBooking,
    onSuccess: async () => {
      setNotice('Varaus luotu.');
      await queryClient.invalidateQueries({ queryKey: ['bookings'] });
      await queryClient.invalidateQueries({ queryKey: ['rooms'] });
    },
    onError: (error: Error) => setNotice(error.message)
  });

  const updateBookingMutation = useMutation({
    mutationFn: ({ id, payload }: { id: string; payload: Parameters<typeof updateBooking>[1] }) => updateBooking(id, payload),
    onSuccess: async () => {
      setNotice('Varaus päivitetty.');
      setEditingBookingId(null);
      await queryClient.invalidateQueries({ queryKey: ['bookings'] });
    },
    onError: (error: Error) => setNotice(error.message)
  });

  const deleteBookingMutation = useMutation({
    mutationFn: deleteBooking,
    onSuccess: async () => {
      setNotice('Varaus peruttu.');
      await queryClient.invalidateQueries({ queryKey: ['bookings'] });
    },
    onError: (error: Error) => setNotice(error.message)
  });

  const availabilityMutation = useMutation({
    mutationFn: searchAvailability,
    onSuccess: (data) => {
      setAvailabilityResults(data);
      setNotice(`Löytyi ${data.length} vapaata huonetta.`);
    },
    onError: (error: Error) => setNotice(error.message)
  });

  const isBusy =
    createCustomerMutation.isPending ||
    deleteCustomerMutation.isPending ||
    createRoomTypeMutation.isPending ||
    createRoomMutation.isPending ||
    deleteRoomMutation.isPending ||
    createBookingMutation.isPending ||
    updateBookingMutation.isPending ||
    deleteBookingMutation.isPending ||
    availabilityMutation.isPending;

  const sortedBookings = useMemo(() => bookingsQuery.data ?? [], [bookingsQuery.data]);

  function submitCustomerForm(event: React.FormEvent<HTMLFormElement>) {
    event.preventDefault();
    createCustomerMutation.mutate({
      firstName: customerForm.firstName,
      lastName: customerForm.lastName,
      email: customerForm.email,
      phone: customerForm.phone,
      notes: customerForm.notes || undefined
    });
  }

  function submitRoomTypeForm(event: React.FormEvent<HTMLFormElement>) {
    event.preventDefault();
    createRoomTypeMutation.mutate({
      name: roomTypeForm.name,
      description: roomTypeForm.description || undefined,
      baseNightlyRate: Number(roomTypeForm.baseNightlyRate),
      maxGuests: Number(roomTypeForm.maxGuests)
    });
  }

  function submitRoomForm(event: React.FormEvent<HTMLFormElement>) {
    event.preventDefault();
    if (!roomForm.roomTypeId) {
      setNotice('Valitse ensin huonetyyppi.');
      return;
    }

    createRoomMutation.mutate({
      roomNumber: roomForm.roomNumber,
      roomTypeId: roomForm.roomTypeId
    });
  }

  function submitBookingForm(event: React.FormEvent<HTMLFormElement>) {
    event.preventDefault();

    if (!bookingForm.customerId || !bookingForm.roomId) {
      setNotice('Valitse ensin asiakas ja huone.');
      return;
    }

    const payload = {
      roomId: bookingForm.roomId,
      checkInDate: bookingForm.checkInDate,
      checkOutDate: bookingForm.checkOutDate,
      guestCount: Number(bookingForm.guestCount),
      specialRequests: bookingForm.specialRequests || undefined
    };

    if (editingBookingId) {
      updateBookingMutation.mutate({ id: editingBookingId, payload });
      return;
    }

    createBookingMutation.mutate({
      customerId: bookingForm.customerId,
      ...payload
    });
  }

  function loadBookingToEdit(booking: Booking) {
    setEditingBookingId(booking.id);
    setBookingForm({
      customerId: booking.customerId,
      roomId: booking.roomId,
      checkInDate: booking.checkInDate,
      checkOutDate: booking.checkOutDate,
      guestCount: String(booking.guestCount),
      specialRequests: booking.specialRequests ?? ''
    });
    setActiveTab('bookings');
    setNotice(`Muokataan varausta ${booking.id.slice(0, 8)}...`);
  }

  function resetBookingForm() {
    setEditingBookingId(null);
    setBookingForm({
      customerId: '',
      roomId: '',
      checkInDate: defaultCheckIn,
      checkOutDate: defaultCheckOut,
      guestCount: '1',
      specialRequests: ''
    });
  }

  function submitAvailabilityForm(event: React.FormEvent<HTMLFormElement>) {
    event.preventDefault();
    availabilityMutation.mutate({
      checkInDate: availabilityForm.checkInDate,
      checkOutDate: availabilityForm.checkOutDate,
      guestCount: availabilityForm.guestCount ? Number(availabilityForm.guestCount) : undefined,
      roomTypeId: availabilityForm.roomTypeId || undefined
    });
  }

  return (
    <main className="app-shell">
      <header className="hero">
        <div>
          <p className="eyebrow">Hotel Lakeview</p>
          <h1>Vastaanoton hallintanäkymä</h1>
          <p className="lede">
            Hallitse asiakkaita, huoneita ja varauksia yhdessä näkymässä. Kausihinnoittelu ja käyttöasteanalytiikka päivittyvät reaaliajassa.
          </p>
        </div>
        <div className="hero-meta">
          <p>Tänään: {format(today, 'yyyy-MM-dd')}</p>
          {notice && <p className="notice">{notice}</p>}
        </div>
      </header>

      <nav className="tabs" aria-label="Päävalikot">
        <button className={activeTab === 'customers' ? 'active' : ''} onClick={() => setActiveTab('customers')}>Asiakkaat</button>
        <button className={activeTab === 'rooms' ? 'active' : ''} onClick={() => setActiveTab('rooms')}>Huoneet</button>
        <button className={activeTab === 'bookings' ? 'active' : ''} onClick={() => setActiveTab('bookings')}>Varaukset</button>
        <button className={activeTab === 'availability' ? 'active' : ''} onClick={() => setActiveTab('availability')}>Saatavuus</button>
        <button className={activeTab === 'reports' ? 'active' : ''} onClick={() => setActiveTab('reports')}>Raportit</button>
      </nav>

      {activeTab === 'customers' && (
        <section className="panel">
          <h2>Asiakkaat</h2>
          <form onSubmit={submitCustomerForm} className="form-grid">
            <input placeholder="Etunimi" value={customerForm.firstName} onChange={(event) => setCustomerForm({ ...customerForm, firstName: event.target.value })} required />
            <input placeholder="Sukunimi" value={customerForm.lastName} onChange={(event) => setCustomerForm({ ...customerForm, lastName: event.target.value })} required />
            <input type="email" placeholder="Sähköposti" value={customerForm.email} onChange={(event) => setCustomerForm({ ...customerForm, email: event.target.value })} required />
            <input placeholder="Puhelin" value={customerForm.phone} onChange={(event) => setCustomerForm({ ...customerForm, phone: event.target.value })} required />
            <input className="full" placeholder="Muistiinpanot" value={customerForm.notes} onChange={(event) => setCustomerForm({ ...customerForm, notes: event.target.value })} />
            <button disabled={isBusy}>Luo asiakas</button>
          </form>

          <ul className="entity-list">
            {customersQuery.data?.map((customer) => (
              <li key={customer.id}>
                <div>
                  <strong>{customer.firstName} {customer.lastName}</strong>
                  <span>{customer.email} - {customer.phone}</span>
                </div>
                <button className="danger" onClick={() => deleteCustomerMutation.mutate(customer.id)} disabled={isBusy}>Poista</button>
              </li>
            ))}
          </ul>
        </section>
      )}

      {activeTab === 'rooms' && (
        <section className="panel two-col">
          <div>
            <h2>Huonetyypit</h2>
            <form onSubmit={submitRoomTypeForm} className="form-grid compact">
              <input placeholder="Nimi" value={roomTypeForm.name} onChange={(event) => setRoomTypeForm({ ...roomTypeForm, name: event.target.value })} required />
              <input type="number" placeholder="Perushinta" value={roomTypeForm.baseNightlyRate} onChange={(event) => setRoomTypeForm({ ...roomTypeForm, baseNightlyRate: event.target.value })} required />
              <input type="number" placeholder="Maksimivieraat" value={roomTypeForm.maxGuests} onChange={(event) => setRoomTypeForm({ ...roomTypeForm, maxGuests: event.target.value })} required />
              <input className="full" placeholder="Kuvaus" value={roomTypeForm.description} onChange={(event) => setRoomTypeForm({ ...roomTypeForm, description: event.target.value })} />
              <button disabled={isBusy}>Luo huonetyyppi</button>
            </form>

            <ul className="entity-list">
              {roomTypesQuery.data?.map((roomType) => (
                <li key={roomType.id}>
                  <div>
                    <strong>{roomType.name}</strong>
                    <span>{roomType.baseNightlyRate} EUR / yö, max {roomType.maxGuests}</span>
                  </div>
                  <button className="danger" onClick={() => deleteRoomTypeMutation.mutate(roomType.id)} disabled={isBusy}>Poista</button>
                </li>
              ))}
            </ul>
          </div>

          <div>
            <h2>Huoneet</h2>
            <form onSubmit={submitRoomForm} className="form-grid compact">
              <input placeholder="Huonenumero" value={roomForm.roomNumber} onChange={(event) => setRoomForm({ ...roomForm, roomNumber: event.target.value })} required />
              <select value={roomForm.roomTypeId} onChange={(event) => setRoomForm({ ...roomForm, roomTypeId: event.target.value })} required>
                <option value="">Valitse huonetyyppi</option>
                {roomTypesQuery.data?.map((roomType) => (
                  <option key={roomType.id} value={roomType.id}>{roomType.name}</option>
                ))}
              </select>
              <button disabled={isBusy}>Luo huone</button>
            </form>

            <ul className="entity-list">
              {roomsQuery.data?.map((room) => (
                <li key={room.id}>
                  <div>
                    <strong>{room.roomNumber}</strong>
                    <span>{room.roomTypeName}</span>
                  </div>
                  <button className="danger" onClick={() => deleteRoomMutation.mutate(room.id)} disabled={isBusy}>Poista</button>
                </li>
              ))}
            </ul>
          </div>
        </section>
      )}

      {activeTab === 'bookings' && (
        <section className="panel two-col">
          <div>
            <h2>{editingBookingId ? 'Muokkaa varausta' : 'Luo varaus'}</h2>
            <form onSubmit={submitBookingForm} className="form-grid compact">
              <select value={bookingForm.customerId} onChange={(event) => setBookingForm({ ...bookingForm, customerId: event.target.value })} required>
                <option value="">Valitse asiakas</option>
                {customersQuery.data?.map((customer) => (
                  <option key={customer.id} value={customer.id}>{customer.firstName} {customer.lastName}</option>
                ))}
              </select>

              <select value={bookingForm.roomId} onChange={(event) => setBookingForm({ ...bookingForm, roomId: event.target.value })} required>
                <option value="">Valitse huone</option>
                {roomsQuery.data?.map((room) => (
                  <option key={room.id} value={room.id}>{room.roomNumber} ({room.roomTypeName})</option>
                ))}
              </select>

              <input type="date" value={bookingForm.checkInDate} onChange={(event) => setBookingForm({ ...bookingForm, checkInDate: event.target.value })} required />
              <input type="date" value={bookingForm.checkOutDate} onChange={(event) => setBookingForm({ ...bookingForm, checkOutDate: event.target.value })} required />
              <input type="number" min="1" max="4" value={bookingForm.guestCount} onChange={(event) => setBookingForm({ ...bookingForm, guestCount: event.target.value })} required />
              <input className="full" placeholder="Erityistoiveet" value={bookingForm.specialRequests} onChange={(event) => setBookingForm({ ...bookingForm, specialRequests: event.target.value })} />

              <button disabled={isBusy}>{editingBookingId ? 'Päivitä varaus' : 'Luo varaus'}</button>
              {editingBookingId && <button type="button" className="ghost" onClick={resetBookingForm}>Peruuta muokkaus</button>}
            </form>
          </div>

          <div>
            <h2>Nykyiset varaukset</h2>
            <ul className="entity-list bookings">
              {sortedBookings.map((booking) => (
                <li key={booking.id}>
                  <div>
                    <strong>{booking.roomNumber} - {booking.customerName}</strong>
                    <span>{booking.checkInDate}{' -> '}{booking.checkOutDate}{' - '}{booking.totalPrice.toFixed(2)} EUR</span>
                  </div>
                  <div className="actions">
                    <button className="ghost" onClick={() => loadBookingToEdit(booking)} disabled={isBusy}>Muokkaa</button>
                    <button className="danger" onClick={() => deleteBookingMutation.mutate(booking.id)} disabled={isBusy}>Peruuta</button>
                  </div>
                </li>
              ))}
            </ul>
          </div>
        </section>
      )}

      {activeTab === 'availability' && (
        <section className="panel">
          <h2>Saatavuushaku</h2>
          <form onSubmit={submitAvailabilityForm} className="form-grid compact">
            <input type="date" value={availabilityForm.checkInDate} onChange={(event) => setAvailabilityForm({ ...availabilityForm, checkInDate: event.target.value })} required />
            <input type="date" value={availabilityForm.checkOutDate} onChange={(event) => setAvailabilityForm({ ...availabilityForm, checkOutDate: event.target.value })} required />
            <input type="number" min="1" max="4" value={availabilityForm.guestCount} onChange={(event) => setAvailabilityForm({ ...availabilityForm, guestCount: event.target.value })} />
            <select value={availabilityForm.roomTypeId} onChange={(event) => setAvailabilityForm({ ...availabilityForm, roomTypeId: event.target.value })}>
              <option value="">Mikä tahansa huonetyyppi</option>
              {roomTypesQuery.data?.map((roomType) => (
                <option key={roomType.id} value={roomType.id}>{roomType.name}</option>
              ))}
            </select>
            <button disabled={isBusy}>Hae</button>
          </form>

          <ul className="entity-list">
            {availabilityResults.map((item) => (
              <li key={item.roomId}>
                <div>
                  <strong>{item.roomNumber} ({item.roomTypeName})</strong>
                  <span>{item.nightlyRate.toFixed(2)} EUR / yö, yhteensä {item.totalPrice.toFixed(2)} EUR</span>
                </div>
              </li>
            ))}
          </ul>
        </section>
      )}

      {activeTab === 'reports' && (
        <section className="panel two-col">
          <div>
            <h2>Raportin aikaväli</h2>
            <div className="form-grid compact">
              <input type="date" value={reportRange.fromDate} onChange={(event) => setReportRange({ ...reportRange, fromDate: event.target.value })} />
              <input type="date" value={reportRange.toDate} onChange={(event) => setReportRange({ ...reportRange, toDate: event.target.value })} />
            </div>

            <h3>Käyttöaste</h3>
            <ul className="entity-list small">
              {occupancyQuery.data?.slice(-10).map((point) => (
                <li key={point.date}>
                  <div>
                    <strong>{point.date}</strong>
                    <span>{point.bookedRooms}/{point.totalRooms} huonetta ({point.occupancyPercent.toFixed(1)}%)</span>
                  </div>
                </li>
              ))}
            </ul>
          </div>

          <div>
            <h3>Liikevaihto jaksoittain</h3>
            <ul className="entity-list small">
              {revenueQuery.data?.map((point) => (
                <li key={point.period}>
                  <div>
                    <strong>{point.period}</strong>
                    <span>{point.revenue.toFixed(2)} EUR</span>
                  </div>
                </li>
              ))}
            </ul>

            <h3>Suosituimmat huonetyypit</h3>
            <ul className="entity-list small">
              {popularityQuery.data?.map((item) => (
                <li key={item.roomTypeId}>
                  <div>
                    <strong>{item.roomTypeName}</strong>
                    <span>{item.bookingCount} varausta, {item.revenue.toFixed(2)} EUR</span>
                  </div>
                </li>
              ))}
            </ul>
          </div>
        </section>
      )}

      {(customersQuery.isError || roomTypesQuery.isError || roomsQuery.isError || bookingsQuery.isError) && (
        <section className="panel error-panel">
          <p>
            API-pyyntö epäonnistui. Varmista, että backend on käynnissä ja CORS on sallittu osoitteelle <code>http://localhost:5173</code>.
          </p>
        </section>
      )}
    </main>
  );
}
