# feature-flags-co

官网 [敏捷开关 https://www.feature-flags.co](https://www.feature-flags.co)

一款开源的产品级功能标记神器

- 高频/精细的发布产品功能
- 安全/无忧的推进社会前进

让功能的发布/下线/回退 精细化到每个市场用户、每一个模块、每一分钟、每一种技术环境、每一个实用场景、每一个运营环节、每一个工作人员


## 使用文档

https://docs.feature-flags.co/

使用文档中不仅包含了使用手册，同时对测试用例、核心算法也有相关描述。


## 版本发布与管理

### SaaS服务的版本发布与管理

main分支中，包含了较新的工作进度中的代码。对于每一个可发布的版本，我们会将代码以Pull Request的方式合并到:

1. `main-mkdocs`分支, 新PR到此分支的代码会通过devops pipeline自动发布到文档服务对应的dev环境，通过approve后会依照顺序发布到uat及prod环境
2. `main-backend`分支, 新PR到此分支的代码会通过devops pipeline自动发布到API服务对应的dev环境，通过approve后会依照顺序发布到uat及prod环境
3. `main-frontend`分支, 新PR到此分支的代码会通过devops pipeline自动发布到前端应用程序对应的dev环境，通过approve后会依照顺序发布到uat及prod环境
4. `main-sql`分支, 新PR到此分支的代码会通过devops pipeline自动发布到SQL数据库对应的dev环境，通过approve后会依照顺序发布到uat及prod环境
5. `main-iac`分支, 新PR到此分支的代码会通过devops pipeline自动将云硬件资源的更新发布dev环境，通过approve后会依照顺序发布到uat及prod环境

### 边缘云与本地版的发布与管理

敬请稍后
