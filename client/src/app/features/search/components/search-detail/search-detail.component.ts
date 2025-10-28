import { ChangeDetectionStrategy, Component, Input } from '@angular/core';

import { PostDetail, SearchHit } from '../../../core/models/search.models';

@Component({
  selector: 'app-search-detail',
  templateUrl: './search-detail.component.html',
  styleUrls: ['./search-detail.component.scss'],
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class SearchDetailComponent {
  @Input() post: PostDetail | null = null;
  @Input() similarPosts: SearchHit[] | null = [];
}
