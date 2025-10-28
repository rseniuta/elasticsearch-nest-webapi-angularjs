import { ChangeDetectionStrategy, Component, Input } from '@angular/core';

interface SearchSummary {
  total: number;
  took: number;
  message: string;
}

@Component({
  selector: 'app-search-summary',
  templateUrl: './search-summary.component.html',
  styleUrls: ['./search-summary.component.scss'],
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class SearchSummaryComponent {
  @Input() summary: SearchSummary | null = null;
  @Input() loading: boolean | null = false;
}
