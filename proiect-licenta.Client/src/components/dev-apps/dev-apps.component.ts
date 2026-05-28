import { Component } from '@angular/core';
import {Button} from 'primeng/button';
import {DatePipe, NgClass, NgForOf} from '@angular/common';
import {Select} from 'primeng/select';
import {CategoriesService, Category} from '../../services/categories.service';
import {App, AppFilters, AppsService} from '../../services/apps.service';
import {ActivatedRoute, Router, RouterLink} from '@angular/router';
import {firstValueFrom} from 'rxjs';
import {FormsModule} from '@angular/forms';

@Component({
  selector: 'app-dev-apps',
  imports: [
    Button,
    DatePipe,
    NgForOf,
    Select,
    FormsModule,
    RouterLink,
    NgClass
  ],
  templateUrl: './dev-apps.component.html',
  styleUrl: './dev-apps.component.scss'
})
export class DevAppsComponent {
  categories: Category[] = [];
  selectedCategories: number[] = [];
  apps: App[] = [];
  filters: AppFilters = {library: false, devApps: true};

  sortOptions = ['Name', 'Price', 'Free', 'Launch-date', 'Rating', 'Discount'];
  order = ['Ascending', 'Descending'];

  constructor(private appService: AppsService, private categoryService: CategoriesService,
              private router: Router, private route: ActivatedRoute) {}

  ngOnInit(){
    this.route.queryParams.subscribe(async params => {
      this.categories = await firstValueFrom(this.categoryService.getAllCategories());

      this.appService.getApps(this.filters).subscribe({
        next: async (data) => {
          this.apps = data;
          await Promise.all(
            this.apps.map(async app => {
              app.rating = await this.getRating(app.id);
              app.icon = await this.appService.getAppIcon(app.id);
            })
          );
        },
        error: (err) => {
          console.error('Error fetching apps', err);
        }
      });
    });
  }

  async getRating(id: number) {
    return this.appService.getAppRating(id);
  }

  isSelected(id: number) {
    return this.selectedCategories.includes(id);
  }

  selectCategory(id: number) {
    if (this.selectedCategories.includes(id))
    {console.log(this.selectedCategories);
      this.selectedCategories = this.selectedCategories.filter(cid => cid != id);} //remove id
    else
      this.selectedCategories = [...this.selectedCategories, id]; //append

    this.filters.categories = this.selectedCategories;

    this.router.navigate([], {
      queryParams: {
        categories: this.selectedCategories.length ? this.selectedCategories.join('_') : null,
        sort: this.filters.sortBy,
        order: this.filters.order
      }
    });
  }

  filterApps(){
    this.router.navigate([], {
      queryParams: {
        category: this.selectedCategories.length ? this.selectedCategories.join('_') : null,
        sort: this.filters.sortBy,
        order: this.filters.order
      }
    })
  }
}
