import { Component, OnInit } from '@angular/core';
import { finalize } from 'rxjs';
import { ProductDto, ProductsService, PaginationDto } from '../../shared/services/products.service';

export interface Product {
  id: string;
  name: string;
  description: string;
}

@Component({
  selector: 'app-products',
  templateUrl: './products.component.html',
  styles: []
})
export class ProductsComponent implements OnInit {
  products: Product[] = [];
  searchQuery = '';
  sortColumn: 'name' | 'description' | 'modulesCount' = 'name';
  sortDirection: 'asc' | 'desc' = 'asc';
  currentPage = 1;
  itemsPerPage = 10;
  totalCount = 0;
  isLoading = false;
  errorMessage = '';

  constructor(private productsService: ProductsService) { }

  ngOnInit(): void {
    this.loadProducts();
  }

  get filteredProducts(): Product[] {
    return [...this.products];
  }

  get totalPages(): number {
    return Math.ceil(this.totalCount / this.itemsPerPage);
  }

  get paginatedProducts(): Product[] {
    return this.products;
  }

  previousPage(): void {
    if (this.currentPage > 1) {
      this.currentPage--;
    }
  }

  nextPage(): void {
    if (this.currentPage < this.totalPages) {
      this.currentPage++;
    }
  }

  sortBy(column: 'name' | 'description' | 'modulesCount'): void {
    if (this.sortColumn === column) {
      this.sortDirection = this.sortDirection === 'asc' ? 'desc' : 'asc';
    } else {
      this.sortColumn = column;
      this.sortDirection = 'asc';
    }

    this.currentPage = 1;
    this.loadProducts();
  }

  onSearchChange(): void {
    this.currentPage = 1;
    this.loadProducts();
  }

  private loadProducts(): void {
    this.isLoading = true;
    this.errorMessage = '';

    const sortQuery = this.sortColumn === 'description'
      ? this.sortDirection === 'asc' ? 'description-asc' : 'description-desc'
      : this.sortDirection === 'asc' ? 'name-asc' : 'name-desc';

    this.productsService.getAllProducts(this.searchQuery, sortQuery, this.currentPage, this.itemsPerPage)
      .pipe(finalize(() => this.isLoading = false))
      .subscribe({
        next: (response: PaginationDto<ProductDto>) => {
          this.products = response.data.map(product => ({
            id: product.id,
            name: product.name,
            description: product.description
          }));
          this.totalCount = response.count;
        },
        error: () => {
          this.products = [];
          this.totalCount = 0;
          this.errorMessage = 'Unable to load products from the API.';
        }
      });
  }

       }
