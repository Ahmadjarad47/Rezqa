import { Component, OnInit, HostListener } from '@angular/core';
import { Router, NavigationEnd } from '@angular/router';
import { filter } from 'rxjs/operators';

interface Notification {
  title: string;
  message: string;
  time: string;
  read: boolean;
}

interface NavItem {
  title: string;
  icon: string;
  route: string;
  children?: NavItem[];
}

@Component({
  selector: 'app-dashboard-layout',
  standalone: false,
  templateUrl: './dashboard-layout.component.html',
  styleUrl: './dashboard-layout.component.css',
})
export class DashboardLayoutComponent implements OnInit {
  isSidebarOpen = true;
  currentRoute: string = '';
  currentPageTitle: string = 'Dashboard';
  isNotificationsOpen = false;
  isProfileMenuOpen = false;

  // Sample notifications - replace with actual data from your service
  notifications: Notification[] = [
    {
      title: 'New Order',
      message: 'You have received a new order #1234',
      time: '5 minutes ago',
      read: false,
    },
    {
      title: 'Product Update',
      message: 'Product "Sample Product" has been updated',
      time: '1 hour ago',
      read: true,
    },
    {
      title: 'System Alert',
      message: 'System maintenance scheduled for tomorrow',
      time: '2 hours ago',
      read: false,
    },
  ];

  navItems: NavItem[] = [
    {
      title: 'Dashboard',
      icon: 'M3 12l2-2m0 0l7-7 7 7M5 10v10a1 1 0 001 1h3m10-11l2 2m-2-2v10a1 1 0 01-1 1h-3m-6 0a1 1 0 001-1v-4a1 1 0 011-1h2a1 1 0 011 1v4a1 1 0 001 1m-6 0h6',
      route: '/admin',
    },
    {
      title: 'Categories',
      icon: 'M19 11H5m14 0a2 2 0 012 2v6a2 2 0 01-2 2H5a2 2 0 01-2-2v-6a2 2 0 012-2m14 0V9a2 2 0 00-2-2M5 11V9a2 2 0 012-2m0 0V5a2 2 0 012-2h6a2 2 0 012 2v2M7 7h10',
      route: '/admin/categories',
    },
    {
      title: 'Products',
      icon: 'M20 7l-8-4-8 4m16 0l-8 4m8-4v10l-8 4m0-10L4 7m8 4v10M4 7v10l8 4',
      route: '/admin/products',
    },
    {
      title: 'Orders',
      icon: 'M16 11V7a4 4 0 00-8 0v4M5 9h14l1 12H4L5 9z',
      route: '/admin/orders',
    },
    {
      title: 'Users',
      icon: 'M12 4.354a4 4 0 110 5.292M15 21H3v-1a6 6 0 0112 0v1zm0 0h6v-1a6 6 0 00-9-5.197M13 7a4 4 0 11-8 0 4 4 0 018 0z',
      route: '/admin/users',
    },
    {
      title: 'Settings',
      icon: 'M10.325 4.317c.426-1.756 2.924-1.756 3.35 0a1.724 1.724 0 002.573 1.066c1.543-.94 3.31.826 2.37 2.37a1.724 1.724 0 001.065 2.572c1.756.426 1.756 2.924 0 3.35a1.724 1.724 0 00-1.066 2.573c.94 1.543-.826 3.31-2.37 2.37a1.724 1.724 0 00-2.572 1.065c-.426 1.756-2.924 1.756-3.35 0a1.724 1.724 0 00-2.573-1.066c-1.543.94-3.31-.826-2.37-2.37a1.724 1.724 0 00-1.065-2.572c-1.756-.426-1.756-2.924 0-3.35a1.724 1.724 0 001.066-2.573c-.94-1.543.826-3.31 2.37-2.37.996.608 2.296.07 2.572-1.065z',
      route: '/admin/settings',
    },
  ];

  constructor(private router: Router) {}

  ngOnInit(): void {
    if (typeof document != 'undefined') {
      this.updateCurrentRoute();
      this.router.events
        .pipe(filter((event) => event instanceof NavigationEnd))
        .subscribe(() => {
          this.updateCurrentRoute();
        });

      // Close dropdowns when clicking outside
      document.addEventListener('click', () => {
        this.isNotificationsOpen = false;
        this.isProfileMenuOpen = false;
      });
    }
  }

  @HostListener('document:click', ['$event'])
  onDocumentClick(event: MouseEvent) {
    // Close dropdowns when clicking outside
    if (!(event.target as HTMLElement).closest('.dropdown-trigger')) {
      this.isNotificationsOpen = false;
      this.isProfileMenuOpen = false;
    }
  }

  toggleSidebar(): void {
    this.isSidebarOpen = !this.isSidebarOpen;
  }

  toggleNotifications(event: Event) {
    event.stopPropagation();
    this.isNotificationsOpen = !this.isNotificationsOpen;
    this.isProfileMenuOpen = false; // Close profile menu when opening notifications
  }

  toggleProfileMenu(event: Event) {
    event.stopPropagation();
    this.isProfileMenuOpen = !this.isProfileMenuOpen;
    this.isNotificationsOpen = false; // Close notifications when opening profile menu
  }

  private updateCurrentRoute(): void {
    this.currentRoute = this.router.url;
    const currentItem = this.navItems.find((item) => this.isActive(item.route));
    this.currentPageTitle = currentItem?.title || 'Dashboard';
  }

  isActive(route: string): boolean {
    return this.currentRoute === route;
  }

  markAllNotificationsAsRead() {
    this.notifications = this.notifications.map((notification) => ({
      ...notification,
      read: true,
    }));
  }
}
