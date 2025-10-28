import { CommonModule } from '@angular/common';
import { NgModule } from '@angular/core';
import { ReactiveFormsModule } from '@angular/forms';
import { MatAutocompleteModule } from '@angular/material/autocomplete';
import { MatButtonModule } from '@angular/material/button';
import { MatCardModule } from '@angular/material/card';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatChipsModule } from '@angular/material/chips';
import { MatIconModule } from '@angular/material/icon';
import { MatInputModule } from '@angular/material/input';
import { MatListModule } from '@angular/material/list';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatTooltipModule } from '@angular/material/tooltip';

import { SearchComponent } from './search.component';
import { SearchFiltersComponent } from './components/search-filters/search-filters.component';
import { SearchInputComponent } from './components/search-input/search-input.component';
import { SearchResultsComponent } from './components/search-results/search-results.component';
import { SearchSummaryComponent } from './components/search-summary/search-summary.component';
import { SearchSuggestionsComponent } from './components/search-suggestions/search-suggestions.component';
import { SearchDetailComponent } from './components/search-detail/search-detail.component';

@NgModule({
  declarations: [
    SearchComponent,
    SearchFiltersComponent,
    SearchInputComponent,
    SearchResultsComponent,
    SearchSummaryComponent,
    SearchSuggestionsComponent,
    SearchDetailComponent
  ],
  imports: [
    CommonModule,
    ReactiveFormsModule,
    MatAutocompleteModule,
    MatButtonModule,
    MatCardModule,
    MatFormFieldModule,
    MatChipsModule,
    MatIconModule,
    MatInputModule,
    MatListModule,
    MatProgressSpinnerModule,
    MatTooltipModule
  ],
  exports: [SearchComponent]
})
export class SearchModule {}
