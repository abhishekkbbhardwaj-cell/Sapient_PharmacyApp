export interface Medicine {
  name: string;
  notes?: string;
  expiryDate: string;
  quantity: number;
  price: number;
  brand: string;
}

export interface AddMedicineRequest {
  name: string;
  notes: string;
  expiryDate: string;
  quantity: number;
  price: number;
  brand: string;
}

export interface MedicineListResponse {
  items: Medicine[];
  pageNumber: number;
  pageSize: number;
  totalCount: number;
  totalPages: number;
}