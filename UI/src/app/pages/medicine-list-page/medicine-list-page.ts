import { CommonModule } from '@angular/common';
import { Component, inject, OnInit, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Medicine, MedicineListResponse } from '../../models/medicine.model';
import { MedicineService } from '../../services/medicine.service';

@Component({
  selector: 'app-medicine-list-page',
  standalone: true,
  imports: [CommonModule, FormsModule],
  styleUrl: './medicine-list-page.css',
  templateUrl: './medicine-list-page.html',
})
export class MedicineListPageComponent implements OnInit {
  private readonly medicineService = inject(MedicineService);

  medicines = signal<Medicine[]>([]);
  searchTerm = '';
  loadError = signal('');
  pageNumber = 1;
  readonly pageSize = 5;
  totalCount = 0;
  totalPages = 0;

  ngOnInit(): void {
    this.loadMedicines();
  }

  loadMedicines(pageNumber = 1): void {
    this.medicineService.getAll(this.searchTerm.trim(), pageNumber, this.pageSize).subscribe({
      next: (response: MedicineListResponse) => {
        this.pageNumber = response.pageNumber;
        this.totalCount = response.totalCount;
        this.totalPages = response.totalPages;
        this.medicines.set(response.items);
        this.loadError.set('');
      },
      error: () => {
        this.medicines.set([]);
        this.totalCount = 0;
        this.totalPages = 0;
        this.loadError.set('Unable to load medicines from the API.');
      },
    });
  }

  searchMedicines(): void {
    this.loadMedicines(1);
  }

  goToPreviousPage(): void {
    if (this.pageNumber > 1) {
      this.loadMedicines(this.pageNumber - 1);
    }
  }

  goToNextPage(): void {
    if (this.pageNumber < this.totalPages) {
      this.loadMedicines(this.pageNumber + 1);
    }
  }

  getRowClass(medicine: Medicine): string {
    if (this.isExpiringSoon(medicine)) {
      return 'row-expiring';
    }

    if (this.isLowStock(medicine)) {
      return 'row-low-stock';
    }

    return '';
  }

  private isExpiringSoon(medicine: Medicine): boolean {
    const expiryParts = medicine.expiryDate.split('-').map(Number);
    if (expiryParts.length !== 3 || expiryParts.some(Number.isNaN)) {
      return false;
    }

    const [expiryYear, expiryMonth, expiryDay] = expiryParts;
    const expiryDate = Date.UTC(expiryYear, expiryMonth - 1, expiryDay);
    const today = new Date();
    const todayDate = Date.UTC(today.getFullYear(), today.getMonth(), today.getDate());
    const diffInDays = Math.floor((expiryDate - todayDate) / (1000 * 60 * 60 * 24));

    return diffInDays >= 0 && diffInDays < 30;
  }

  private isLowStock(medicine: Medicine): boolean {
    return medicine.quantity < 10;
  }
}
