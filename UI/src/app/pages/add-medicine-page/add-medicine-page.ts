import { CommonModule } from '@angular/common';
import { Component, inject } from '@angular/core';
import {
  AbstractControl,
  FormBuilder,
  ReactiveFormsModule,
  ValidationErrors,
  Validators,
} from '@angular/forms';
import { RouterLink } from '@angular/router';
import { Medicine } from '../../models/medicine.model';
import { MedicineService } from '../../services/medicine.service';

function nonBlank(control: AbstractControl): ValidationErrors | null {
  return typeof control.value === 'string' && control.value.trim().length > 0
    ? null
    : { blank: true };
}

function wholeNumber(control: AbstractControl): ValidationErrors | null {
  return Number.isInteger(Number(control.value)) ? null : { wholeNumber: true };
}

function futureDate(control: AbstractControl): ValidationErrors | null {
  const today = new Date().toISOString().split('T')[0];
  return typeof control.value === 'string' && control.value > today
    ? null
    : { futureDate: true };
}

@Component({
  selector: 'app-add-medicine-page',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, RouterLink],
  styleUrl: './add-medicine-page.css',
  templateUrl: './add-medicine-page.html',
})
export class AddMedicinePageComponent {
  private readonly medicineService = inject(MedicineService);
  private readonly formBuilder = inject(FormBuilder);

  readonly today = new Date().toISOString().split('T')[0];
  isSubmitting = false;
  submitError = '';
  submitSuccess = '';

  readonly medicineForm = this.formBuilder.nonNullable.group({
    name: ['', [Validators.required, nonBlank, Validators.minLength(2), Validators.maxLength(200)]],
    brand: ['', [Validators.required, nonBlank, Validators.minLength(2), Validators.maxLength(200)]],
    expiryDate: ['', [Validators.required, futureDate]],
    quantity: [0, [Validators.required, Validators.min(0), Validators.max(10000000), wholeNumber]],
    price: [0, [Validators.required, Validators.min(0.01)]],
    notes: ['', [Validators.maxLength(1000)]],
  });

  get notesLength(): number {
    return this.medicineForm.controls.notes.value.length;
  }

  submitMedicine(): void {
    this.submitError = '';
    this.submitSuccess = '';

    if (this.medicineForm.invalid) {
      this.medicineForm.markAllAsTouched();
      this.submitError = 'Please correct the highlighted validation errors.';
      return;
    }

    this.isSubmitting = true;
    const values = this.medicineForm.getRawValue();
    const payload: Medicine = {
      name: values.name.trim(),
      notes: values.notes.trim(),
      expiryDate: values.expiryDate,
      quantity: values.quantity,
      price: values.price,
      brand: values.brand.trim(),
    };

    this.medicineService.add(payload).subscribe({
      next: () => {
        this.submitSuccess = 'Medicine added successfully.';
        this.medicineForm.reset({ name: '', brand: '', expiryDate: '', quantity: 0, price: 0, notes: '' });
        this.isSubmitting = false;
      },
      error: () => {
        this.submitError = 'Unable to save medicine. Check the form values and try again.';
        this.isSubmitting = false;
      },
    });
  }
}
