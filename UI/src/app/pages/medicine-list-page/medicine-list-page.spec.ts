import { TestBed } from '@angular/core/testing';
import { of, throwError } from 'rxjs';
import { Medicine, MedicineListResponse } from '../../models/medicine.model';
import { MedicineService } from '../../services/medicine.service';
import { MedicineListPageComponent } from './medicine-list-page';

const medicines: Medicine[] = [
  {
    name: 'Paracetamol',
    brand: 'Generic',
    expiryDate: '2099-12-31',
    quantity: 25,
    price: 25,
    notes: 'Pain relief',
  },
];

function page(items = medicines, pageNumber = 1, totalPages = 1): MedicineListResponse {
  return {
    items,
    pageNumber,
    pageSize: 5,
    totalCount: items.length,
    totalPages,
  };
}

describe('MedicineListPageComponent', () => {
  let component: MedicineListPageComponent;
  let medicineService: { getAll: ReturnType<typeof vi.fn> };

  beforeEach(() => {
    medicineService = { getAll: vi.fn().mockReturnValue(of(page())) };

    TestBed.configureTestingModule({
      imports: [MedicineListPageComponent],
      providers: [{ provide: MedicineService, useValue: medicineService }],
    });

    component = TestBed.createComponent(MedicineListPageComponent).componentInstance;
  });

  it('creates the component', () => {
    expect(component).toBeTruthy();
  });

  it('loads the first page on initialization', () => {
    component.ngOnInit();
    expect(medicineService.getAll).toHaveBeenCalledWith('', 1, 5);
  });

  it('stores items returned by the API', () => {
    component.loadMedicines();
    expect(component.medicines()).toEqual(medicines);
  });

  it('stores pagination metadata returned by the API', () => {
    medicineService.getAll.mockReturnValue(of(page(medicines, 2, 3)));
    component.loadMedicines(2);
    expect(component.pageNumber).toBe(2);
    expect(component.totalPages).toBe(3);
    expect(component.totalCount).toBe(1);
  });

  it('clears data and exposes an error when loading fails', () => {
    medicineService.getAll.mockReturnValue(throwError(() => new Error('network error')));
    component.loadMedicines();
    expect(component.medicines()).toEqual([]);
    expect(component.loadError()).toContain('Unable to load medicines');
  });

  it('passes the trimmed search query to the API', () => {
    component.searchTerm = '  para  ';
    component.searchMedicines();
    expect(medicineService.getAll).toHaveBeenCalledWith('para', 1, 5);
  });

  it('resets search results to page one', () => {
    component.searchTerm = 'para';
    component.pageNumber = 3;
    component.searchMedicines();
    expect(medicineService.getAll).toHaveBeenCalledWith('para', 1, 5);
  });

  it('loads the previous page when available', () => {
    component.pageNumber = 2;
    component.totalPages = 3;
    component.goToPreviousPage();
    expect(medicineService.getAll).toHaveBeenCalledWith('', 1, 5);
  });

  it('does not load a previous page from page one', () => {
    component.pageNumber = 1;
    component.goToPreviousPage();
    expect(medicineService.getAll).not.toHaveBeenCalled();
  });

  it('loads the next page when available', () => {
    component.pageNumber = 1;
    component.totalPages = 3;
    component.goToNextPage();
    expect(medicineService.getAll).toHaveBeenCalledWith('', 2, 5);
  });

  it('does not load a next page from the last page', () => {
    component.pageNumber = 3;
    component.totalPages = 3;
    component.goToNextPage();
    expect(medicineService.getAll).not.toHaveBeenCalled();
  });

  it('marks expiring medicines red', () => {
    const expiringMedicine = { ...medicines[0], expiryDate: '2026-09-25' };
    expect(component.getRowClass(expiringMedicine)).toBe('row-expiring');
  });

  it('marks low-stock medicines yellow', () => {
    const lowStockMedicine = { ...medicines[0], quantity: 5, expiryDate: '2099-12-31' };
    expect(component.getRowClass(lowStockMedicine)).toBe('row-low-stock');
  });

  it('prioritizes expiry warning over low stock warning', () => {
    const medicine = { ...medicines[0], quantity: 5, expiryDate: '2026-09-25' };
    expect(component.getRowClass(medicine)).toBe('row-expiring');
  });
});
