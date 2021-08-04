import { Component, OnInit } from '@angular/core';
import { Subject } from 'rxjs';
import { IAuthProps } from 'src/app/config/types';
import { IAccountUser } from 'src/app/config/types';
import { TeamService } from 'src/app/services/team.service';
import { AccountService } from 'src/app/services/account.service';
import { getAuth } from 'src/app/utils';

@Component({
  selector: 'app-team',
  templateUrl: './team.component.html',
  styleUrls: ['./team.component.less']
})
export class TeamComponent implements OnInit {

  private destory$: Subject<void> = new Subject();

  public searhTerm: string = '';

  public teamMembers: IAccountUser[] = [];
  public searchResult: IAccountUser[] = [];
  public isInitLoading: boolean = true;                  // 数据加载中对话
  public auth: IAuthProps;

  addMemberVisible: boolean = false;
  currentAccountId: number;
  isAccountOwner: boolean;

  constructor(
    private accountService: AccountService,
    private teamService: TeamService
  ) {}

  ngOnInit(): void {
    this.auth = getAuth();
    const currentAccountProjectEnv = this.accountService.getCurrentAccountProjectEnv();
    this.currentAccountId = currentAccountProjectEnv.account.id;
    this.initTeamMembers(this.currentAccountId);
  }

  isMemberDeleteBtnVisible(member: IAccountUser): boolean {
    return this.isAccountOwner && this.auth.email !== member.email;
  }

  doSearch() {
    if (this.searhTerm === '' || this.searhTerm == null) {
      this.searchResult = this.teamMembers;
    } else {
      this.searchResult = this.teamMembers.filter(member => member.email.toLowerCase().indexOf(this.searhTerm.toLowerCase()) !== -1)
    }
  }

  private initTeamMembers(accountId: number) {
    this.isInitLoading = true;
    this.refreshTeamMembers(accountId);
  }

  ngOnDestroy(): void {
    this.destory$.next();
    this.destory$.complete();
  }

  addMember(){
    this.addMemberVisible = true;
  }

  onDeleteMemberClick(member: IAccountUser) {
    this.teamService.removeMember(this.currentAccountId, member.userId).subscribe(() => {
      this.refreshTeamMembers(this.currentAccountId);
    })
  }

  refreshTeamMembers(accountId: number) {
    this.teamService.getMembers(accountId).subscribe((result: IAccountUser[]) => {
      this.isInitLoading = false;
      if(result.length) {
        this.teamMembers = result;
        this.isAccountOwner = !!this.teamMembers.find(x => x.email === this.auth.email && x.role === 'Owner');
        this.doSearch();
      } else {
        // This should never happen
      }
    });
  }

  memberClosed() {
    this.addMemberVisible = false;
    this.refreshTeamMembers(this.currentAccountId);
  }
}
