import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';
import { Medicine, MedicineListResponse } from '../models/medicine.model';
import { MedicineService } from './medicine.service';

describe('MedicineService', () => {
  let service: MedicineService;
  let httpMock: HttpTestingController;
  const response: MedicineListResponse = {
    items: [],
    pageNumber: 1,
    pageSize: 10,
    totalCount: 0,
    totalPages: 0,
  };
  const medicine: Medicine = {
    name: 'Paracetamol',
    brand: 'Generic',
    expiryDate: '2099-12-31',
    quantity: 20,
    price: 25,
    notes: 'Pain relief',
  };

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [MedicineService, provideHttpClient(), provideHttpClientTesting()],
    });

    service = TestBed.inject(MedicineService);
    httpMock = TestBed.inject(HttpTestingController);
  });

  afterEach(() => {
    httpMock.verify();
  });

  it('creates the service', () => {
    expect(service).toBeTruthy();
  });

  it('requests medicines with GET', () => {
    service.getAll().subscribe();
    const request = httpMock.expectOne((request) => request.url.includes('/api/medicine'));
    expect(request.request.method).toBe('GET');
    request.flush(response);
  });

  it('uses page one by default', () => {
    service.getAll().subscribe();
    const request = httpMock.expectOne((request) => request.url.includes('/api/medicine'));
    expect(request.request.params.get('pageNumber')).toBe('1');
    expect(request.request.params.get('pageSize')).toBe('10');
    request.flush(response);
  });

  it('sends a search query with pagination parameters', () => {
    service.getAll('para', 2, 5).subscribe();
    const request = httpMock.expectOne((request) => request.url.includes('/api/medicine'));
    expect(request.request.params.get('query')).toBe('para');
    expect(request.request.params.get('pageNumber')).toBe('2');
    expect(request.request.params.get('pageSize')).toBe('5');
    request.flush(response);
  });

  it('encodes query values through HttpParams', () => {
    service.getAll('pain relief', 1, 5).subscribe();
    const request = httpMock.expectOne((request) => request.url.includes('/api/medicine'));
    expect(request.request.params.get('query')).toBe('pain relief');
    request.flush(response);
  });

  it('returns the paginated response', () => {
    let result: MedicineListResponse | undefined;
    service.getAll().subscribe((value) => (result = value));
    const request = httpMock.expectOne((request) => request.url.includes('/api/medicine'));
    request.flush(response);
    expect(result).toEqual(response);
  });

  it('posts a new medicine', () => {
    service.add(medicine).subscribe();
    const request = httpMock.expectOne((request) => request.url.includes('/api/medicine'));
    expect(request.request.method).toBe('POST');
    request.flush(medicine);
  });

  it('sends the medicine as the POST body', () => {
    service.add(medicine).subscribe();
    const request = httpMock.expectOne((request) => request.url.includes('/api/medicine'));
    expect(request.request.body).toEqual(medicine);
    request.flush(medicine);
  });

  it('returns the created medicine', () => {
    let result: Medicine | undefined;
    service.add(medicine).subscribe((value) => (result = value));
    const request = httpMock.expectOne((request) => request.url.includes('/api/medicine'));
    request.flush(medicine);
    expect(result).toEqual(medicine);
  });

  it('propagates GET errors', () => {
    let error: unknown;
    service.getAll().subscribe({ error: (value) => (error = value) });
    const request = httpMock.expectOne((request) => request.url.includes('/api/medicine'));
    request.flush('failure', { status: 500, statusText: 'Server Error' });
    expect(error).toBeTruthy();
  });

  it('propagates POST errors', () => {
    let error: unknown;
    service.add(medicine).subscribe({ error: (value) => (error = value) });
    const request = httpMock.expectOne((request) => request.url.includes('/api/medicine'));
    request.flush('failure', { status: 400, statusText: 'Bad Request' });
    expect(error).toBeTruthy();
  });
});
