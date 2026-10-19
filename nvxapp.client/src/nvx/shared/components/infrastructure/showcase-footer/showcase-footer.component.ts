import { Component, ChangeDetectionStrategy } from '@angular/core';
import { NavController } from '@ionic/angular/lazy';
import { MainMenuItem, MenuType } from '../../../../Utility/infrastructure/main-menu.service';
import { ShowcaseService } from '../../../../Utility/infrastructure/showcase.service';

// Pie' di pagina delle pagine dello showcase: voci del menu dello showcase e nome del prodotto
@Component({
  selector: 'app-showcase-footer',
  templateUrl: './showcase-footer.component.html',
  changeDetection: ChangeDetectionStrategy.Eager,
  standalone: false
})
export class ShowcaseFooterComponent {

  public readonly anno = new Date().getFullYear();

  constructor(public showcaseService: ShowcaseService,
              private navCtrl: NavController) { }

  public get voci(): MainMenuItem[] {
    return this.showcaseService.Pages
      .filter(x => x.menuType === MenuType.MenuItem)
      .sort((a, b) => a.group - b.group || a.zorder - b.zorder);
  }

  public vai(url: string): void {
    this.navCtrl.navigateRoot(url);
  }
}
