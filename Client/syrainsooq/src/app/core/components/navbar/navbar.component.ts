import { Component, HostListener, OnInit, OnDestroy } from '@angular/core';
import { Router, NavigationEnd } from '@angular/router';
import { filter, Subscription, debounceTime, distinctUntilChanged, switchMap } from 'rxjs';
import { AuthService } from 'src/app/identity/services/auth.service';
import { UserData } from 'src/app/identity/models/auth.models';
import { WishlistService } from '@app/dashboard/components/wishlist/wishlist.service';
import { HomeService, CategoryDto } from '@app/core/services/home.service';
import { FormControl } from '@angular/forms';

@Component({
  selector: 'app-navbar',
  standalone: false,
  templateUrl: './navbar.component.html',
  styleUrl: './navbar.component.css',
})
export class NavbarComponent implements OnInit, OnDestroy {
  isDarkMode = false;
  userData: UserData | null = null;
  isDropdownOpen = false;
  showNavbar = true;
  private routerSubscription!: Subscription;
  private searchSubscription!: Subscription;

  // Search functionality
  searchControl = new FormControl('');
  searchResults: CategoryDto[] = [];
  isSearching = false;
  showSearchResults = false;
  allCategories: CategoryDto[] = [];

  constructor(
    public authService: AuthService,
    public wishlistService: WishlistService,
    private router: Router,
    private homeService: HomeService
  ) {
    // Check for saved theme preference or use system preference
    if (typeof window !== 'undefined') {
      const savedTheme = localStorage.getItem('theme');
      if (savedTheme) {
        this.isDarkMode = savedTheme === 'dark';
        document.documentElement.classList.toggle('dark', this.isDarkMode);
      } else if (window.matchMedia('(prefers-color-scheme: dark)').matches) {
        this.isDarkMode = true;
        document.documentElement.classList.add('dark');
      }
    }
    // Subscribe to user data
    this.authService.userData$.subscribe(data => {
      this.userData = data;
    });
  }

  ngOnInit() {
    this.wishlistService.getWishlist().subscribe();
    
    // Load all categories for search
    this.loadAllCategories();
    
    // Setup search functionality
    this.setupSearch();
    
    // Monitor route changes to hide navbar on dashboard pages
    this.routerSubscription = this.router.events
      .pipe(filter(event => event instanceof NavigationEnd))
      .subscribe((event: NavigationEnd) => {
        this.showNavbar = !event.url.includes('dashboard');
      });
    
    // Check initial URL
    this.showNavbar = !this.router.url.includes('dashboard');
  }

  ngOnDestroy() {
    if (this.routerSubscription) {
      this.routerSubscription.unsubscribe();
    }
    if (this.searchSubscription) {
      this.searchSubscription.unsubscribe();
    }
  }

  private loadAllCategories(): void {
    // Load a larger number of categories for search
    this.homeService.getCategories(1, 100).subscribe({
      next: (result) => {
        this.allCategories = result.items;
      },
      error: (error) => {
        console.error('Error loading categories for search:', error);
      }
    });
  }

  private setupSearch(): void {
    this.searchSubscription = this.searchControl.valueChanges
      .pipe(
        debounceTime(300), // Wait 300ms after user stops typing
        distinctUntilChanged(), // Only emit if value has changed
      )
      .subscribe(searchTerm => {
        this.performSearch(searchTerm || '');
      });
  }

  private performSearch(searchTerm: string): void {
    if (!searchTerm.trim()) {
      this.searchResults = [];
      this.showSearchResults = false;
      return;
    }

    this.isSearching = true;
    
    // Filter categories based on search term
    const filteredResults = this.allCategories.filter(category =>
      category.title.toLowerCase().includes(searchTerm.toLowerCase()) ||
      category.description.toLowerCase().includes(searchTerm.toLowerCase())
    );

    // Limit results to first 10 for better UX
    this.searchResults = filteredResults.slice(0, 10);
    this.showSearchResults = this.searchResults.length > 0;
    this.isSearching = false;
  }

  onSearchFocus(): void {
    if (this.searchControl.value && this.searchResults.length > 0) {
      this.showSearchResults = true;
    }
  }

  onSearchBlur(): void {
    // Delay hiding results to allow clicking on them
    setTimeout(() => {
      this.showSearchResults = false;
    }, 200);
  }

  onSearchResultClick(category: CategoryDto): void {
    this.homeService.setCategory(category);
    this.searchControl.setValue('');
    this.showSearchResults = false;
  }

  onSearchSubmit(): void {
    const searchTerm = this.searchControl.value;
    if (searchTerm && searchTerm.trim()) {
      // Navigate to search results page or perform search
      this.router.navigate(['/search'], { queryParams: { q: searchTerm } });
      this.showSearchResults = false;
    }
  }

  clearSearch(): void {
    this.searchControl.setValue('');
    this.searchResults = [];
    this.showSearchResults = false;
  }

  toggleTheme(): void {
    this.isDarkMode = !this.isDarkMode;
    document.documentElement.classList.toggle('dark');
    localStorage.setItem('theme', this.isDarkMode ? 'dark' : 'light');
  }

  toggleDropdown(): void {
    this.isDropdownOpen = !this.isDropdownOpen;
  }

  closeDropdown(): void {
    this.isDropdownOpen = false;
  }

  @HostListener('document:click', ['$event'])
  onDocumentClick(event: Event): void {
    const target = event.target as HTMLElement;
    if (!target.closest('.dropdown-container')) {
      this.closeDropdown();
    }
    // Close search results when clicking outside
    if (!target.closest('.search-container')) {
      this.showSearchResults = false;
    }
  }
}
