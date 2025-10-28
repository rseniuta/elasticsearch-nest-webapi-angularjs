import { ChangeDetectionStrategy, Component, EventEmitter, Input, Output } from '@angular/core';

export interface TagAggregation {
  tag: string;
  count: number;
}

@Component({
  selector: 'app-search-filters',
  templateUrl: './search-filters.component.html',
  styleUrls: ['./search-filters.component.scss'],
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class SearchFiltersComponent {
  @Input() filters: TagAggregation[] | null = [];
  @Input() activeFilters: ReadonlySet<string> | null = null;
  @Output() filterToggled = new EventEmitter<string>();

  isActive(tag: string): boolean {
    return this.activeFilters ? this.activeFilters.has(tag) : false;
  }

  onToggle(tag: string): void {
    this.filterToggled.emit(tag);
  }
}
