import { Component, Inject, OnInit, PLATFORM_ID } from '@angular/core';
import { CategoryService } from '../../services/category.service';
import {
  CategoryDto,
  CreateCategoryRequest,
  UpdateCategoryRequest,
} from '../../models/category.model';
import {
  PaginatedResponse,
  PaginationParams,
} from '../../models/pagination.model';
import { finalize } from 'rxjs/operators';
import { isPlatformBrowser } from '@angular/common';

@Component({
  selector: 'app-category',
  standalone: false,
  templateUrl: './category.component.html',
  styleUrl: './category.component.css',
})
export class CategoryComponent implements OnInit {
  // Make Math available in template
  protected readonly Math = Math;

  categories: CategoryDto[] = [];
  loading = false;
  showModal = false;
  showDeleteModal = false;
  isEditing = false;
  formData: CreateCategoryRequest | UpdateCategoryRequest = {
    title: '',
    description: '',
    image: null as any,
  };
  editingId: string | null = null;
  imagePreview: string | null = null;
  deletingId: string | null = null;
  selectedCategory: CategoryDto | null = null;

  // Pagination state
  currentPage = 1;
  pageSize = 3;
  totalCount = 0;
  totalPages = 0;
  hasPreviousPage = false;
  hasNextPage = false;

  // Search state
  searchQuery = '';
  private searchTimeout: any;

  constructor(
    private categoryService: CategoryService,
    @Inject(PLATFORM_ID) private platformId: any
  ) {}

  ngOnInit(): void {
    if (isPlatformBrowser(this.platformId)) {
      this.loadCategories();
    }
  }

  loadCategories(): void {
    this.loading = true;
    const params: PaginationParams = {
      pageNumber: this.currentPage,
      pageSize: this.pageSize,
      search: this.searchQuery || undefined,
    };

    this.categoryService
      .getAll(params)
      .pipe(finalize(() => (this.loading = false)))
      .subscribe({
        next: (response: PaginatedResponse<CategoryDto>) => {
          this.categories = response.items;
          this.totalCount = response.totalCount;
          this.totalPages = response.totalPages;
          this.hasPreviousPage = response.hasPreviousPage;
          this.hasNextPage = response.hasNextPage;
        },
        error: (error) => console.error('Error loading categories:', error),
      });
  }

  onSearch(event: Event): void {
    const input = event.target as HTMLInputElement;
    this.searchQuery = input.value.trim();

    // Clear any existing timeout
    if (this.searchTimeout) {
      clearTimeout(this.searchTimeout);
    }

    // Set a new timeout to debounce the search
    this.searchTimeout = setTimeout(() => {
      this.currentPage = 1; // Reset to first page when searching
      this.loadCategories();
    }, 300); // 300ms debounce
  }

  onPageChange(page: number): void {
    this.currentPage = page;
    this.loadCategories();
  }

  openCreateModal(): void {
    this.isEditing = false;
    this.formData = {
      title: '',
      description: '',
      image: null as any,
    };
    this.imagePreview = null;
    this.showModal = true;
  }

  openEditModal(category: CategoryDto): void {
    this.isEditing = true;
    this.editingId = category.id;
    this.formData = {
      id: category.id,
      title: category.title,
      description: category.description,
    };
    this.imagePreview = category.image;
    this.showModal = true;
  }

  closeModal(): void {
    this.showModal = false;
    this.isEditing = false;
    this.editingId = null;
    this.formData = {
      title: '',
      description: '',
      image: null as any,
    };
    this.imagePreview = null;
  }

  onImageSelected(event: Event): void {
    const file = (event.target as HTMLInputElement).files?.[0];
    if (file) {
      this.formData.image = file;
      // Create preview
      const reader = new FileReader();
      reader.onload = () => {
        this.imagePreview = reader.result as string;
      };
      reader.readAsDataURL(file);
    }
  }

  onSubmit(): void {
    if (this.isEditing && this.editingId) {
      this.categoryService
        .update(this.editingId, this.formData as UpdateCategoryRequest)
        .subscribe({
          next: () => {
            this.loadCategories();
            this.closeModal();
          },
          error: (error) => console.error('Error updating category:', error),
        });
    } else {
      this.categoryService
        .create(this.formData as CreateCategoryRequest)
        .subscribe({
          next: () => {
            this.loadCategories();
            this.closeModal();
          },
          error: (error) => console.error('Error creating category:', error),
        });
    }
  }

  openDeleteModal(category: CategoryDto): void {
    this.deletingId = category.id;
    this.selectedCategory = category;
    this.showDeleteModal = true;
  }

  closeDeleteModal(): void {
    this.showDeleteModal = false;
    this.deletingId = null;
    this.selectedCategory = null;
  }

  confirmDelete(): void {
    if (this.deletingId) {
      this.categoryService.delete(this.deletingId).subscribe({
        next: () => {
          this.loadCategories();
          this.closeDeleteModal();
        },
        error: (error) => console.error('Error deleting category:', error),
      });
    }
  }

  shouldShowPageButton(pageNumber: number): boolean {
    // Always show first and last page
    if (pageNumber === 1 || pageNumber === this.totalPages) {
      return true;
    }

    // Show pages around current page
    const range = 1; // Number of pages to show before and after current page
    return Math.abs(pageNumber - this.currentPage) <= range;
  }

  shouldShowEllipsis(pageNumber: number): boolean {
    // Show ellipsis between gaps in page numbers
    if (pageNumber === 1 || pageNumber === this.totalPages) {
      return false;
    }

    const range = 1;
    const isGap = Math.abs(pageNumber - this.currentPage) > range;
    const isNotFirstOrLast = pageNumber !== 1 && pageNumber !== this.totalPages;

    return isGap && isNotFirstOrLast;
  }
}
