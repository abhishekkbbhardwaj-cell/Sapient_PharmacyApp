import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { Medicine, MedicineListResponse } from '../models/medicine.model';
import { environment } from '../../environments/environment';

@Injectable({ providedIn: 'root' })
export class MedicineService {
  private readonly http = inject(HttpClient);
  private readonly apiUrl = environment.apiUrl;

  getAll(query = '', pageNumber = 1, pageSize = 10): Observable<MedicineListResponse> {
    return this.http.get<MedicineListResponse>(this.apiUrl, {
      params: { query, pageNumber, pageSize },
    });
  }

  add(medicine: Medicine): Observable<Medicine> {
    return this.http.post<Medicine>(this.apiUrl, medicine);
  }
}