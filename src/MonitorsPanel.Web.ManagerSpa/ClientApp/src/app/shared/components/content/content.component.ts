import { Component, OnInit, ChangeDetectionStrategy } from '@angular/core';

@Component({
  selector: 'r-content',
  templateUrl: './content.component.html',
  styleUrls: ['./content.component.scss'],
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class ContentComponent implements OnInit {

  constructor() { }

  ngOnInit() {
  }

}
