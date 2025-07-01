import { Component, OnInit, ChangeDetectionStrategy } from '@angular/core';
import { Router } from '@angular/router';
import { CategoryDto, HomeService } from '@app/core/services/home.service';
import { AuthService } from '@app/identity/services/auth.service';

@Component({
  selector: 'app-home',
  standalone: false,
  templateUrl: './home.component.html',
  styleUrls: ['./home.component.css'],
})
export class HomeComponent implements OnInit {
  isAuthenticated = false;
  categories: CategoryDto[] = [];
  loading = false;

  // متغيرات للصفحات
  pageNumber = 1;
  pageSize = 6;
  totalPages = 1;

  constructor(
    private router: Router,
    private authService: AuthService,
    public homeService: HomeService,
  ) {}

  ngOnInit(): void {
    this.isAuthenticated = this.authService.isAuthenticated();
    this.fetchCategories();
  }

  fetchCategories(loadMore: boolean = false): void {
    this.loading = true;
    this.homeService.getCategories(this.pageNumber, this.pageSize).subscribe({
      next: (result) => {
        this.totalPages = result.totalPages;
        if (loadMore) {
          this.categories = [...this.categories, ...result.items];
        } else {
          this.categories = result.items;
        }
        this.loading = false;
      },
      error: (err) => {
        this.loading = false;
        // يمكنك إضافة إشعار بالخطأ هنا
      }
    });
  }

  onLoadMore(): void {
    if (this.pageNumber < this.totalPages && !this.loading) {
      this.pageNumber++;
      this.fetchCategories(true);
    }
  }

  onGetStarted(): void {
    if (this.isAuthenticated) {
      this.router.navigate(['/dashboard']);
    } else {
      this.router.navigate(['/all']);
    }
  }

  onLearnMore(): void {
    // Implement learn more functionality
    console.log('Learn more clicked');
  }
}
