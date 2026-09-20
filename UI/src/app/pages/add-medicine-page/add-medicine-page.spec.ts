import { TestBed } from '@angular/core/testing';
import { provideRouter } from '@angular/router';
import { of, throwError } from 'rxjs';
import { AddMedicinePageComponent } from './add-medicine-page';
import { MedicineService } from '../../services/medicine.service';

function validForm(component: AddMedicinePageComponent): void {
  component.medicineForm.setValue({
    name: 'Paracetamol 500mg',
    brand: 'Generic',
    expiryDate: '2099-12-31',
    quantity: 25,
    price: 25.5,
    notes: 'Pain relief',
  });
}

describe('AddMedicinePageComponent', () => {
  let component: AddMedicinePageComponent;
  let medicineService: { add: ReturnType<typeof vi.fn> };

  beforeEach(() => {
    medicineService = { add: vi.fn() };

    TestBed.configureTestingModule({
      imports: [AddMedicinePageComponent],
      providers: [provideRouter([]), { provide: MedicineService, useValue: medicineService }],
    });

    component = TestBed.createComponent(AddMedicinePageComponent).componentInstance;
  });

  it('creates the component', () => {
    expect(component).toBeTruthy();
  });

  it('starts with an invalid form', () => {
    expect(component.medicineForm.invalid).toBe(true);
  });

  it('requires a medicine name', () => {
    expect(component.medicineForm.controls.name.hasError('required')).toBe(true);
  });

  it('rejects whitespace-only medicine names', () => {
    component.medicineForm.controls.name.setValue('   ');
    expect(component.medicineForm.controls.name.hasError('blank')).toBe(true);
  });

  it('rejects names shorter than two characters', () => {
    component.medicineForm.controls.name.setValue('A');
    expect(component.medicineForm.controls.name.hasError('minlength')).toBe(true);
  });

  it('rejects brands longer than 200 characters', () => {
    component.medicineForm.controls.brand.setValue('B'.repeat(201));
    expect(component.medicineForm.controls.brand.hasError('maxlength')).toBe(true);
  });

  it('rejects an expiry date that is not in the future', () => {
    component.medicineForm.controls.expiryDate.setValue(component.today);
    expect(component.medicineForm.controls.expiryDate.hasError('futureDate')).toBe(true);
  });

  it('rejects negative quantity values', () => {
    component.medicineForm.controls.quantity.setValue(-1);
    expect(component.medicineForm.controls.quantity.hasError('min')).toBe(true);
  });

  it('rejects fractional quantities', () => {
    component.medicineForm.controls.quantity.setValue(1.5);
    expect(component.medicineForm.controls.quantity.hasError('wholeNumber')).toBe(true);
  });

  it('rejects a zero price', () => {
    component.medicineForm.controls.price.setValue(0);
    expect(component.medicineForm.controls.price.hasError('min')).toBe(true);
  });

  it('rejects notes longer than 1000 characters', () => {
    component.medicineForm.controls.notes.setValue('N'.repeat(1001));
    expect(component.medicineForm.controls.notes.hasError('maxlength')).toBe(true);
  });

  it('marks the form touched instead of submitting invalid data', () => {
    component.submitMedicine();
    expect(component.medicineForm.touched).toBe(true);
    expect(medicineService.add).not.toHaveBeenCalled();
  });

  it('submits a trimmed valid payload', () => {
    medicineService.add.mockReturnValue(of({}));
    validForm(component);

    component.submitMedicine();

    expect(medicineService.add).toHaveBeenCalledWith({
      name: 'Paracetamol 500mg',
      brand: 'Generic',
      expiryDate: '2099-12-31',
      quantity: 25,
      price: 25.5,
      notes: 'Pain relief',
    });
  });

  it('resets the form and shows success after submission', () => {
    medicineService.add.mockReturnValue(of({}));
    validForm(component);

    component.submitMedicine();

    expect(component.submitSuccess).toBe('Medicine added successfully.');
    expect(component.medicineForm.controls.name.value).toBe('');
    expect(component.isSubmitting).toBe(false);
  });

  it('shows an error when the API rejects the submission', () => {
    medicineService.add.mockReturnValue(throwError(() => new Error('network error')));
    validForm(component);

    component.submitMedicine();

    expect(component.submitError).toContain('Unable to save medicine');
    expect(component.isSubmitting).toBe(false);
  });
});
