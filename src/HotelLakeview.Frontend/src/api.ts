export type Customer = {
  id: string;
  firstName: string;
  lastName: string;
  email: string;
  phone: string;
  notes?: string | null;
};

export type RoomType = {
  id: string;
  name: string;
  description?: string | null;
  baseNightlyRate: number;
  maxGuests: number;
  isActive: boolean;
};

export type Room = {
  id: string;
  roomNumber: string;
  roomTypeId: string;
  roomTypeName: string;
  status: number;
  isActive: boolean;
};

export type Booking = {
  id: string;
  customerId: string;
  customerName: string;
  roomId: string;
  roomNumber: string;
  roomTypeId: string;
  roomTypeName: string;
  checkInDate: string;
  checkOutDate: string;
  status: number;
  nightlyRateSnapshot: number;
  totalPrice: number;
  specialRequests?: string | null;
  guestCount: number;
};

export type AvailabilityResult = {
  roomId: string;
  roomNumber: string;
  roomTypeId: string;
  roomTypeName: string;
  nightlyRate: number;
  totalPrice: number;
};

export type OccupancyPoint = {
  date: string;
  bookedRooms: number;
  totalRooms: number;
  occupancyPercent: number;
};

export type RevenuePoint = {
  period: string;
  revenue: number;
};

export type PopularRoomType = {
  roomTypeId: string;
  roomTypeName: string;
  bookingCount: number;
  revenue: number;
};

type HttpMethod = 'GET' | 'POST' | 'PUT' | 'DELETE';

const baseUrl = import.meta.env.VITE_API_BASE_URL ?? 'http://localhost:5000';

async function request<T>(path: string, method: HttpMethod = 'GET', body?: unknown): Promise<T> {
  const response = await fetch(`${baseUrl}${path}`, {
    method,
    headers: body ? { 'Content-Type': 'application/json' } : undefined,
    body: body ? JSON.stringify(body) : undefined
  });

  if (!response.ok) {
    const message = await response.text();
    throw new Error(message || `Request failed with status ${response.status}`);
  }

  if (response.status === 204) {
    return undefined as T;
  }

  return (await response.json()) as T;
}

export async function fetchCustomers(): Promise<Customer[]> {
  return request<Customer[]>('/api/customers');
}

export async function createCustomer(input: Omit<Customer, 'id'>): Promise<Customer> {
  return request<Customer>('/api/customers', 'POST', input);
}

export async function deleteCustomer(id: string): Promise<void> {
  return request<void>(`/api/customers/${id}`, 'DELETE');
}

export async function fetchRoomTypes(): Promise<RoomType[]> {
  return request<RoomType[]>('/api/room-types');
}

export async function createRoomType(input: {
  name: string;
  description?: string;
  baseNightlyRate: number;
  maxGuests: number;
}): Promise<RoomType> {
  return request<RoomType>('/api/room-types', 'POST', input);
}

export async function deleteRoomType(id: string): Promise<void> {
  return request<void>(`/api/room-types/${id}`, 'DELETE');
}

export async function fetchRooms(): Promise<Room[]> {
  return request<Room[]>('/api/rooms');
}

export async function createRoom(input: { roomNumber: string; roomTypeId: string }): Promise<Room> {
  return request<Room>('/api/rooms', 'POST', input);
}

export async function deleteRoom(id: string): Promise<void> {
  return request<void>(`/api/rooms/${id}`, 'DELETE');
}

export async function fetchBookings(): Promise<Booking[]> {
  return request<Booking[]>('/api/bookings');
}

export async function createBooking(input: {
  customerId: string;
  roomId: string;
  checkInDate: string;
  checkOutDate: string;
  guestCount: number;
  specialRequests?: string;
}): Promise<Booking> {
  return request<Booking>('/api/bookings', 'POST', input);
}

export async function updateBooking(
  id: string,
  input: {
    roomId: string;
    checkInDate: string;
    checkOutDate: string;
    guestCount: number;
    specialRequests?: string;
  }
): Promise<Booking> {
  return request<Booking>(`/api/bookings/${id}`, 'PUT', input);
}

export async function deleteBooking(id: string): Promise<void> {
  return request<void>(`/api/bookings/${id}`, 'DELETE');
}

export async function searchAvailability(input: {
  checkInDate: string;
  checkOutDate: string;
  guestCount?: number;
  roomTypeId?: string;
}): Promise<AvailabilityResult[]> {
  return request<AvailabilityResult[]>('/api/availability/search', 'POST', input);
}

export async function fetchOccupancy(fromDate: string, toDate: string): Promise<OccupancyPoint[]> {
  return request<OccupancyPoint[]>(`/api/analytics/occupancy?fromDate=${fromDate}&toDate=${toDate}`);
}

export async function fetchRevenue(fromDate: string, toDate: string): Promise<RevenuePoint[]> {
  return request<RevenuePoint[]>(`/api/analytics/revenue?fromDate=${fromDate}&toDate=${toDate}`);
}

export async function fetchPopularRoomTypes(fromDate: string, toDate: string): Promise<PopularRoomType[]> {
  return request<PopularRoomType[]>(`/api/analytics/popular-room-types?fromDate=${fromDate}&toDate=${toDate}`);
}
