import { ChangeDetectionStrategy, Component, EventEmitter, Input, Output } from '@angular/core';

@Component({
  selector: 'app-search-suggestions',
  templateUrl: './search-suggestions.component.html',
  styleUrls: ['./search-suggestions.component.scss'],
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class SearchSuggestionsComponent {
  @Input() suggestions: string[] | null = [];
  @Output() selected = new EventEmitter<string>();

  onSuggestionClick(value: string): void {
    this.selected.emit(value);
  }
}
