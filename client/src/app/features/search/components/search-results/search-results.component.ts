import { ChangeDetectionStrategy, Component, EventEmitter, Input, Output } from '@angular/core';

import { SearchHit } from '../../../core/models/search.models';

@Component({
  selector: 'app-search-results',
  templateUrl: './search-results.component.html',
  styleUrls: ['./search-results.component.scss'],
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class SearchResultsComponent {
  @Input() results: SearchHit[] = [];
  @Input() loading: boolean | null = false;
  @Output() selected = new EventEmitter<SearchHit>();

  onSelect(result: SearchHit): void {
    this.selected.emit(result);
  }
}
