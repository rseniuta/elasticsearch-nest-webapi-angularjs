import { ChangeDetectionStrategy, Component, EventEmitter, Input, Output } from '@angular/core';
import { FormGroup } from '@angular/forms';

@Component({
  selector: 'app-search-input',
  templateUrl: './search-input.component.html',
  styleUrls: ['./search-input.component.scss'],
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class SearchInputComponent {
  @Input() formGroup!: FormGroup;
  @Input() autocompleteOptions: string[] | null = [];
  @Input() isLoading: boolean | null = false;

  @Output() submitted = new EventEmitter<void>();
  @Output() autocompleteSelected = new EventEmitter<string>();

  onSubmit(): void {
    this.submitted.emit();
  }

  onOptionSelected(value: string): void {
    this.autocompleteSelected.emit(value);
  }
}
