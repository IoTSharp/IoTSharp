import { Component, OnInit } from '@angular/core';
import { SettingsService } from '@delon/theme';

@Component({
  selector: 'app-i18nlist',
  templateUrl: './i18nlist.component.html',
  styleUrls: ['./i18nlist.component.less'],
})
export class I18nlistComponent implements OnInit {
  constructor(private settingService: SettingsService) {}

  ngOnInit(): void {}
}
