import { Component, OnDestroy, OnInit, SecurityContext } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { FfcService } from 'src/app/services/ffc.service';
import { NzMessageService } from 'ng-zorro-antd/message';
import { SwitchService } from 'src/app/services/switch.service';
import { CSwitchParams, IVariationOption } from '../types/switch-new';
import { ZeroCodeService } from 'src/app/services/zero-code.service';
import { ICssSelectorItem, IZeroCode } from '../types/zero-code';
import { uuidv4 } from 'src/app/utils';
import { IProjectEnv } from 'src/app/config/types';
import { NzConfigService } from 'ng-zorro-antd/core/config';
import { DomSanitizer } from '@angular/platform-browser';

@Component({
  selector: 'zero-code-settings',
  templateUrl: './zero-code-settings.component.html',
  styleUrls: ['./zero-code-settings.component.less']
})
export class ZeroCodeSettingsComponent implements OnInit, OnDestroy {

  compareWith: (obj1: any, obj2: any) => boolean = (obj1: any, obj2: any) => {
    if(obj1 && obj2) {
      return obj1.localId === obj2.localId;
    } else {
      return false;
    }
  };

  featureFlagId: string;
  currentVariationOptions: IVariationOption[] = [];
  isLoading = true;

  model: IZeroCode;

  actionOptions: {[key: string]: string}[] = [
    { value: 'show', label: "显示页面元素"}, { value: 'modify', label: '修改页面元素'}
  ];

  constructor(
    private route: ActivatedRoute,
    private switchServe: SwitchService,
    private zeroCodeService: ZeroCodeService,
    private message: NzMessageService,
    private nzConfigService: NzConfigService,
    private sanitizer: DomSanitizer
  ) {
    // set monaco editor mode
    const defaultEditorOption = this.nzConfigService.getConfigForComponent('codeEditor')?.defaultEditorOption || {};
    this.nzConfigService.set('codeEditor', {
      defaultEditorOption: {
        ...defaultEditorOption,
        theme:'vs-dark'
      }
    });

    this.featureFlagId = decodeURIComponent(this.route.snapshot.params['id']);
    const currentProjectEnv: IProjectEnv = JSON.parse(localStorage.getItem('current-project'));
    this.model = {
      envId: currentProjectEnv.envId,
      envSecret: currentProjectEnv.envSecret,
      isActive: true,
      featureFlagId: this.featureFlagId,
      featureFlagKey: this.featureFlagId.split('__')[4],
      items: []
    }

    this.switchServe.getSwitchDetail(this.featureFlagId).subscribe(res => {
      const featureDetail = new CSwitchParams(res);
      this.currentVariationOptions = featureDetail.getVariationOptions();
    });
  }

  ngOnDestroy(): void {

  }

  ngOnInit(): void {
    if(this.switchServe.envId) {
      this.initData();
    }
  }

  itemActionChanged(item: ICssSelectorItem) {
    if (item.action === 'show') {
      item.htmlContent = null;
      item.htmlProperties = [];
      item.style = '.ffc-style {\r\n\r\n}';
    }
  }

  private initData() {
    this.zeroCodeService.getZeroCodes(this.switchServe.envId, this.featureFlagId).subscribe((res: IZeroCode) => {
      if (res) {
        this.model = res;
        this.model.items = res.items.map(itm => {
          return Object.assign({}, itm, {
            htmlProperties: itm.htmlProperties || [],
            style: !!itm.style ? itm.style : '.ffc-style {\r\n\r\n}'
          });
        })
      }

      this.isLoading = false;
    }, err => {
      this.message.error('数据加载失败');
      this.isLoading = false;
    })
  }

  addHtmlProperty(item: ICssSelectorItem) {
    item.htmlProperties = [...item.htmlProperties, { id: uuidv4(), name: null, value: null}];
  }

  removeHtmlProperty(item: ICssSelectorItem, id: string) {
    item.htmlProperties = item.htmlProperties.filter(d => d.id !== id);
  }

  onAddCssSelectorRow() {
    this.model.items = [
      ...this.model.items,
      {
        id: uuidv4(),
        cssSelector: null,
        description: null,
        variationOption: null,
        action: null,
        htmlProperties: [],
        htmlContent: null,
        style: '.ffc-style {\r\n\r\n}',
        url: null
      }
    ];
  }

  deleteCssSelectorRow(id: string): void {
    this.model.items = this.model.items.filter(d => d.id !== id);
  }

  isSaving: boolean = false;
  doSubmit() {
    let styleHasError = false;
    const data = Object.assign({}, this.model);
    data.items = this.model.items.map(itm => {
      styleHasError ||= !!itm.style && !itm.style.startsWith('.ffc-style {');
      return Object.assign({}, itm, {
        style: this.sanitizer.sanitize(SecurityContext.STYLE, itm.style || ''),
        htmlProperties: itm.htmlProperties?.map(p => Object.assign({}, p, {
          name: this.sanitizer.sanitize(SecurityContext.HTML, p.name || ''),
          value: this.sanitizer.sanitize(SecurityContext.HTML, p.value || '')
        }))
      });
    });

    if (styleHasError) {
      this.message.error('请保留 包裹 CSS 样式的 .ffc-style 部分');
      return;
    }

    this.isSaving = true;

    this.zeroCodeService.upsert(data)
      .subscribe(
        res => {
          this.isSaving = false;
          this.message.success('更新成功！');
        },
        err => {
          this.message.error('发生错误，请重试！');
          this.isSaving = false;
        }
      );
  }
}
