import { Routes } from '@angular/router';
import { AddMedicinePageComponent } from './pages/add-medicine-page/add-medicine-page';
import { MedicineListPageComponent } from './pages/medicine-list-page/medicine-list-page';

export const routes: Routes = [
  { path: '', component: MedicineListPageComponent },
  { path: 'add', component: AddMedicinePageComponent },
  { path: '**', redirectTo: '' },
];