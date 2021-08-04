import { Component, OnDestroy, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import { NzMessageService } from 'ng-zorro-antd/message';
import { Subject } from 'rxjs';
import { SwitchService } from 'src/app/services/switch.service';
import { IFfParams } from '../types/switch-new';
import { AccountService } from 'src/app/services/account.service';

@Component({
  selector: 'index',
  templateUrl: './switch-index.component.html',
  styleUrls: ['./switch-index.component.less']
})
export class SwitchIndexComponent implements OnInit, OnDestroy {

  private destory$: Subject<void> = new Subject();
  private currentAccountId: number;

  nameSearchValue: string = '';
  showType: '' | 'Enabled' | 'Disabled' = '';

  public switchLists: IFfParams[] = [];
  public createModalVisible: boolean = false;             // 创建开关的弹窗显示
  public isOkLoading: boolean = false;                    // 创建开关加载中动画
  public isInitLoading: boolean = true;                  // 数据加载中对话
  public switchName: string = '';
  public isIntoing: boolean = false;                      // 是否点击了一条开关，防止路由切换慢的双击效果
  public totalCount: number = 0;

  constructor(
    private router: Router,
    public switchServe: SwitchService,
    private msg: NzMessageService,
    private accountService: AccountService
  ) {}

  ngOnInit(): void {
    const currentAccountProjectEnv = this.accountService.getCurrentAccountProjectEnv();
    this.currentAccountId = currentAccountProjectEnv.account.id;
    const envId = currentAccountProjectEnv.projectEnv.envId;
    this.switchServe.envId = envId;
    this.initSwitchList(envId)
  }

  private initSwitchList(id: number) {
    this.isInitLoading = true;
    this.switchServe.getSwitchList(id).subscribe((result: IFfParams[]) => {
        this.isInitLoading = false;
        if(result.length) {
          this.switchLists = result;
        } else {
          //this.msg.info("当前 Project 没有开关，请添加!");
          this.switchLists = [];
          //this.createModalVisible = true;
        }
    })
  }

  // 添加开关
  addSwitch() {
    this.createModalVisible = true;
  }

  // 切换开关状态
  onChangeSwitchStatus(data: IFfParams, status: 'Enabled' | 'Disabled', event: MouseEvent) {
    event.stopPropagation && event.stopPropagation();
      if(data.status === status){
        return;
      } else {
        this.switchServe.changeSwitchStatus(data.id, status)
          .subscribe(_ => {
            this.msg.success("开关状态已切换!");
          }, _ => {
            this.msg.error("开关状态切换失败!");
          });
        data.status = status;
      }
  }

  ngOnDestroy(): void {
    this.destory$.next();
    this.destory$.complete();
  }

  // 关闭弹窗
  public handleCancel() {
    this.createModalVisible = false;
  }

  public handleOk() {
    if(!this.switchName.length) {
      this.msg.error("请输入开关名字!");
      return;
    }
    this.isOkLoading = true;

    this.switchServe.createNewSwitch(this.switchName)
      .subscribe((result: IFfParams) => {
        this.switchServe.setCurrentSwitch(result);
        this.toRouter(result.id);
        this.isOkLoading = false;
    }, _ => {
      this.msg.error("创建开关失败，请查看是否有相同名字的开关!");
      this.isOkLoading = false;
    })
  }

  // 点击进入对应开关详情
  public onIntoSwitchDetail(data: IFfParams) {
    if(this.isIntoing) return;
    this.isIntoing = true;
    this.switchServe.setCurrentSwitch(data);
    this.toRouter(data.id);
  }

  // 路由跳转
  private toRouter(id: string) {
    this.router.navigateByUrl("/switch-manage/condition/" + encodeURIComponent(id));
  }

  // 转换本地时间
  getLocalDate(date: string) {
    if (!date) return '';
    return new Date(date);
  }
}
