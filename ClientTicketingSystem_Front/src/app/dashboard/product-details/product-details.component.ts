import { Component, OnInit } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { finalize } from 'rxjs';
import { ModuleDto, ProductDto, ProductsService, PaginationDto } from '../../shared/services/products.service';

@Component({
  selector: 'app-product-details',
  templateUrl: './product-details.component.html',
  styleUrls: ['./product-details.component.css']
})
export class ProductDetailsComponent implements OnInit {
  product: ProductDto | null = null;
  modules: ModuleDto[] = [];
  searchQuery = '';
  sortColumn: 'name' | 'description' = 'name';
  sortDirection: 'asc' | 'desc' = 'asc';
  isLoading = false;
  errorMessage = '';
  currentProductId: string | null = null;
  currentPage = 1;
  itemsPerPage = 10;
  totalCount = 0;
  showCreateModuleModal = false;
  createModuleName = '';
  createModuleDescription = '';
  createModuleSubmitted = false;
  isModuleCreating = false;
  showEditModuleModal = false;
  showDeleteModuleModal = false;
  selectedModule: ModuleDto | null = null;
  editModuleName = '';
  editModuleDescription = '';
  isModuleSaving = false;
  isModuleDeleting = false;

  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private productsService: ProductsService
  ) { }

  ngOnInit(): void {
    const id = this.route.snapshot.paramMap.get('id');
    if (!id) {
      this.errorMessage = 'Product not found.';
      return;
    }

    this.currentProductId = id;
    this.loadProduct(id);
    this.loadModules();
  }

  get totalPages(): number {
    return Math.ceil(this.totalCount / this.itemsPerPage);
  }

  get paginatedModules(): ModuleDto[] {
    return this.modules;
  }

  sortBy(column: 'name' | 'description'): void {
    if (this.sortColumn === column) {
      this.sortDirection = this.sortDirection === 'asc' ? 'desc' : 'asc';
    } else {
      this.sortColumn = column;
      this.sortDirection = 'asc';
    }

    this.currentPage = 1;
    this.loadModules();
  }

  onSearchChange(): void {
    this.currentPage = 1;
    this.loadModules();
  }

  getSortDirection(column: 'name' | 'description'): 'asc' | 'desc' | '' {
    if (this.sortColumn !== column) {
      return '';
    }

    return this.sortDirection;
  }

  openCreateModule(): void {
    this.showCreateModuleModal = true;
    this.createModuleSubmitted = false;
  }

  closeCreateModule(): void {
    this.showCreateModuleModal = false;
    this.createModuleName = '';
    this.createModuleDescription = '';
    this.createModuleSubmitted = false;
    this.isModuleCreating = false;
  }

  saveCreateModule(): void {
    this.createModuleSubmitted = true;

    const name = this.createModuleName.trim();
    const description = this.createModuleDescription.trim();
    const productId = this.product?.id;

    if (!productId || !name || !description) {
      return;
    }

    this.isModuleCreating = true;
    this.productsService.createModule({
      name,
      description,
      produtId: productId
    }).pipe(finalize(() => this.isModuleCreating = false)).subscribe({
      next: () => {
        this.closeCreateModule();
        this.loadModules();
      },
      error: () => {
        this.errorMessage = 'Failed to create module.';
      }
    });
  }

  openEditModule(module: ModuleDto): void {
    this.selectedModule = { ...module };
    this.editModuleName = module.name;
    this.editModuleDescription = module.description;
    this.showEditModuleModal = true;
  }

  closeEditModule(): void {
    this.showEditModuleModal = false;
    this.selectedModule = null;
    this.editModuleName = '';
    this.editModuleDescription = '';
    this.isModuleSaving = false;
  }

  saveModuleEdit(): void {
    if (!this.selectedModule) {
      return;
    }

    const name = this.editModuleName.trim();
    const description = this.editModuleDescription.trim();
    if (!name || !description) {
      return;
    }

    this.isModuleSaving = true;
    this.productsService.updateModule({
      id: this.selectedModule.id,
      name,
      description
    }).pipe(finalize(() => this.isModuleSaving = false)).subscribe({
      next: () => {
        this.closeEditModule();
        this.loadModules();
      },
      error: () => {
        this.errorMessage = 'Failed to update module.';
      }
    });
  }

  openDeleteModule(module: ModuleDto): void {
    this.selectedModule = { ...module };
    this.showDeleteModuleModal = true;
  }

  closeDeleteModule(): void {
    this.showDeleteModuleModal = false;
    this.selectedModule = null;
    this.isModuleDeleting = false;
  }

  confirmDeleteModule(): void {
    if (!this.selectedModule) {
      return;
    }

    this.isModuleDeleting = true;
    this.productsService.deleteModule(this.selectedModule.id)
      .pipe(finalize(() => this.isModuleDeleting = false))
      .subscribe({
        next: (deleted) => {
          if (deleted) {
            this.loadModules();
            if (this.currentPage > this.totalPages && this.currentPage > 1) {
              this.currentPage--;
            }
          }
          this.closeDeleteModule();
        },
        error: () => {
          this.errorMessage = 'Failed to delete module.';
        }
      });
  }


  loadProduct(id: string): void {
    this.isLoading = true;
    this.errorMessage = '';

    this.productsService.getProductById(id)
      .pipe(finalize(() => this.isLoading = false))
      .subscribe({
        next: (product) => {
          if (!product) {
            this.errorMessage = 'Product not found.';
            return;
          }

          this.product = product;
        },
        error: () => {
          this.errorMessage = 'Unable to load product details.';
        }
      });
  }

  private loadModules(): void {
    if (!this.currentProductId) {
      return;
    }

    this.isLoading = true;
    this.errorMessage = '';

    const sortQuery = this.sortColumn === 'description'
      ? this.sortDirection === 'asc' ? 'description-asc' : 'description-desc'
      : this.sortDirection === 'asc' ? 'name-asc' : 'name-desc';

    this.productsService.getAllModules(this.searchQuery, sortQuery, this.currentProductId, this.currentPage, this.itemsPerPage)
      .pipe(finalize(() => this.isLoading = false))
      .subscribe({
        next: (response: PaginationDto<ModuleDto>) => {
          this.modules = response.data;
          this.totalCount = response.count;
        },
        error: () => {
          this.modules = [];
          this.totalCount = 0;
          this.errorMessage = 'Unable to load modules.';
        }
      });
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

  goBack(): void {
    void this.router.navigate(['/dashboard/products']);
  }

}
