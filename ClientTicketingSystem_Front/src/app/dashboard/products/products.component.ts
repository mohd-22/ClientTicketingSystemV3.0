import { Component, OnInit } from '@angular/core';
import { finalize } from 'rxjs';
import { ProductDto, ProductsService, PaginationDto } from '../../shared/services/products.service';

export interface Product {
  id: string;
  name: string;
  description: string;
  modulesCount: number;
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
  showEditModal = false;
  editingProduct: Product | null = null;
  editName = '';
  editDescription = '';
  isSaving = false;
  showCreateModal = false;
  createName = '';
  createDescription = '';
  isCreating = false;
  createSubmitted = false;
  showDeleteModal = false;
  deletingProduct: Product | null = null;
  isDeleting = false;
  editSubmitted = false;

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
      : this.sortColumn === 'modulesCount'
        ? this.sortDirection === 'asc' ? 'modules-asc' : 'modules-desc'
        : this.sortDirection === 'asc' ? 'name-asc' : 'name-desc';

    this.productsService.getAllProducts(this.searchQuery, sortQuery, this.currentPage, this.itemsPerPage)
      .pipe(finalize(() => this.isLoading = false))
      .subscribe({
        next: (response: PaginationDto<ProductDto>) => {
          this.products = response.data.map(product => ({
            id: product.id,
            name: product.name,
            description: product.description,
            modulesCount: (product as any).modules?.length ?? (product as any).Modules?.length ?? (product as any).modulesCount ?? (product as any).ModulesCount ?? 0
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

  openEdit(product: Product): void {
    this.editingProduct = { ...product };
    this.editName = product.name;
    this.editDescription = product.description;
    this.editSubmitted = false;
    this.showEditModal = true;
  }

  closeEdit(): void {
    this.showEditModal = false;
    this.editingProduct = null;
    this.editName = '';
    this.editDescription = '';
    this.isSaving = false;
    this.editSubmitted = false;
  }

  openCreate(): void {
    this.createSubmitted = false;
    this.showCreateModal = true;
  }

  closeCreate(): void {
    this.showCreateModal = false;
    this.createName = '';
    this.createDescription = '';
    this.isCreating = false;
    this.createSubmitted = false;
  }

  openDelete(product: Product): void {
    this.deletingProduct = { ...product };
    this.showDeleteModal = true;
  }

  closeDelete(): void {
    this.showDeleteModal = false;
    this.deletingProduct = null;
    this.isDeleting = false;
  }

  saveEdit(): void {
    this.editSubmitted = true;

    if (!this.editingProduct || !this.editName.trim() || !this.editDescription.trim()) {
      return;
    }

    this.isSaving = true;
    const payload = { id: this.editingProduct.id, name: this.editName.trim(), description: this.editDescription.trim() };
    this.productsService.updateProduct(payload).pipe(finalize(() => this.isSaving = false)).subscribe({
      next: (updated) => {
        if (updated) {
          const idx = this.products.findIndex(p => p.id === updated.id);
          if (idx >= 0) {
            this.products[idx].name = updated.name;
            this.products[idx].description = updated.description;
                this.products[idx].modulesCount = (updated as any).modules?.length ?? (updated as any).Modules?.length ?? (updated as any).modulesCount ?? (updated as any).ModulesCount ?? this.products[idx].modulesCount;
          }
        }
        this.closeEdit();
      },
      error: () => {
        this.errorMessage = 'Failed to update product.';
      }
    });
  }

  saveCreate(): void {
    this.createSubmitted = true;
    const name = this.createName.trim();
    const description = this.createDescription.trim();

    if (!name || !description) {
      return;
    }

    this.isCreating = true;
    this.errorMessage = '';

    this.productsService.createProduct({ name, description })
      .pipe(finalize(() => this.isCreating = false))
      .subscribe({
        next: () => {
          this.closeCreate();
          this.loadProducts();
        },
        error: () => {
          this.errorMessage = 'Failed to create product.';
        }
      });
  }

  confirmDelete(): void {
    if (!this.deletingProduct) return;
    this.isDeleting = true;

    this.productsService.deleteProduct(this.deletingProduct.id)
      .pipe(finalize(() => this.isDeleting = false))
      .subscribe({
        next: (deleted) => {
          if (deleted) {
            this.products = this.products.filter(product => product.id !== this.deletingProduct?.id);
            if (this.currentPage > this.totalPages && this.currentPage > 1) {
              this.currentPage--;
            }
          }
          this.closeDelete();
        },
        error: () => {
          this.errorMessage = 'Failed to delete product.';
        }
      });
  }
}
