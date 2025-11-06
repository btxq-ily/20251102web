# 我的知识小栈 - 个人博客/笔记管理系统

计算机232班777组
祝宋女士天天开心😊
ASP.NET Core MVC 综合训练项目

## 项目简介

这是一个功能完整的博客/笔记管理系统，涵盖用户认证、文章CRUD、评论互动、标签分类、图片上传等核心功能。

### 技术栈

- **后端**：ASP.NET Core MVC 9.0
- **ORM**：Entity Framework Core 9.0
- **数据库**：SQL Server 2022 (兼容 2012+)
- **认证**：Cookie Authentication
- **密码加密**：BCrypt.Net
- **前端**：Razor + Bootstrap 5 + Bootstrap Icons
- **Markdown**：Markdig
- **交互**：原生 JavaScript (Fetch API)

### 核心功能

#### 必做功能（已完成）
- ✅ **用户认证**（30分）
  - 注册：用户名、邮箱、密码（BCrypt 哈希）
  - 登录/登出：Cookie 会话保持
  - 权限控制：仅作者可编辑/删除自己的文章

- ✅ **文章 CRUD**（30分）
  - 创建：Markdown 编辑器，支持标签
  - 读取：列表页（分页5条/页）、详情页（Markdown 渲染）
  - 更新：编辑文章与标签
  - 删除：确认对话框，级联删除评论

- ✅ **前端交互**（20分）
  - 响应式布局：Bootstrap Grid，适配手机/PC
  - 动态交互：AJAX 点赞（无刷新）、删除确认、表单验证

- ✅ **数据持久化**（10分）
  - SQL Server 2022，EF Core Code First
  - 6张表：Users、Posts、Comments、Tags、PostTags、PostLikes
  - 外键关联、唯一索引、级联删除策略

#### 扩展功能（已完成，加分项）
- ✅ **标签系统**：多对多关系，按标签筛选
- ✅ **搜索功能**：标题+内容关键词搜索
- ✅ **评论功能**：登录用户可评论
- ✅ **图片上传**：支持 JPG/PNG/GIF/WEBP，最大 5MB
- ✅ **个人中心**：统计信息、我的文章列表
- ✅ **自定义仓储层**：`IRepository<T>` / `Repository<T>` 通用数据访问

---

## 快速开始

### 前置要求

- .NET SDK 9.0+
- SQL Server 2022（或 2012+ 兼容版本）
- 浏览器（推荐 Chrome/Edge）

### 安装步骤

1. **克隆项目**
```bash
git clone <your-repo-url>
cd web
```

2. **配置数据库连接**

复制配置文件模板并编辑：
```bash
cd KnowledgeStack.Web
cp appsettings.example.json appsettings.json
```

编辑 `appsettings.json`，将 `YOUR_SERVER` 改为你的 SQL Server 实例名（例如：`localhost` 或 `localhost\\SQLEXPRESS`）。

3. **创建数据库**
```bash
cd KnowledgeStack.Web
dotnet ef database update
```

4. **运行项目**
```bash
dotnet run --urls http://localhost:5080
```

5. **访问系统**

打开浏览器访问：http://localhost:5080

---

## 使用说明

### 首次使用

1. 点击右上角"注册"创建账号
2. 登录后进入"个人中心"查看统计信息
3. 点击"新建文章"开始写作（支持 Markdown）
4. 在"标签管理"创建标签，发文时可选择标签

### 主要功能

- **文章管理**
  - 列表：搜索、标签筛选、分页浏览
  - 详情：Markdown 渲染、点赞、评论
  - 编辑/删除：仅作者可操作

- **图片上传**
  - 在新建/编辑文章页点击"上传图片"
  - 自动插入 Markdown 图片语法
  - 支持 JPG、PNG、GIF、WEBP，最大 5MB

- **互动功能**
  - 点赞：详情页点击"点赞"按钮（AJAX 无刷新）
  - 评论：登录用户可在文章下评论

---

## 数据库设计

### ER 关系图（核心表）

```
Users (用户表)
├── Id (PK, IDENTITY)
├── Username (唯一索引)
├── Email (唯一索引)
├── PasswordHash (BCrypt)
└── CreatedAt

Posts (文章表)
├── Id (PK, IDENTITY)
├── Title
├── Content (Markdown)
├── CreatedAt
├── UpdatedAt
└── UserId (FK → Users, CASCADE)

Comments (评论表)
├── Id (PK, IDENTITY)
├── Content
├── CreatedAt
├── UserId (FK → Users, RESTRICT)
└── PostId (FK → Posts, CASCADE)

Tags (标签表)
├── Id (PK, IDENTITY)
└── Name

PostTags (文章-标签关联表，多对多)
├── PostId (PK, FK → Posts, CASCADE)
└── TagId (PK, FK → Tags, CASCADE)

PostLikes (点赞表)
├── PostId (PK, FK → Posts, CASCADE)
├── UserId (PK, FK → Users, RESTRICT)
└── CreatedAt
```

### 外键策略说明

- **Cascade**：删除文章时，自动删除关联的评论、标签、点赞
- **Restrict**：避免 SQL Server 多重级联路径冲突

---

## 项目结构

```
KnowledgeStack.Web/
├── Controllers/          # 控制器
│   ├── AccountController.cs    # 注册/登录/登出
│   ├── PostsController.cs      # 文章CRUD、评论、点赞
│   ├── TagsController.cs       # 标签管理
│   ├── ProfileController.cs    # 个人中心
│   └── UploadController.cs     # 图片上传
├── Data/
│   └── Repositories/     # 自定义仓储层 ⭐
│       ├── IRepository.cs
│       └── Repository.cs
├── Models/               # 数据模型
│   ├── Entities.cs       # User/Post/Comment/Tag/PostTag/PostLike
│   └── AppDbContext.cs   # EF Core 上下文
├── Services/             # 业务逻辑 ⭐
│   └── AuthService.cs    # 注册/登录验证
├── Views/                # Razor 视图
│   ├── Account/          # 登录/注册
│   ├── Posts/            # 文章列表/详情/新建/编辑/删除
│   ├── Tags/             # 标签管理
│   ├── Profile/          # 个人中心
│   └── Shared/
│       └── _Layout.cshtml  # 全局布局
├── wwwroot/              # 静态资源
│   ├── css/site.css      # 自定义样式
│   ├── uploads/          # 用户上传图片
│   └── lib/              # Bootstrap、jQuery
└── Program.cs            # 启动配置

⭐ 标注的是"自定义类"，满足加分要求
```

---

## 自定义类说明（加分项）

### 1. 通用仓储层 `IRepository<T>` / `Repository<T>`

**位置**：`Data/Repositories/`

**作用**：
- 封装 EF Core 的 CRUD 操作
- 支持条件查询、排序、Include 导航属性
- 便于单元测试和替换实现

**关键方法**：
```csharp
Task<T?> GetByIdAsync(object id);
Task<IEnumerable<T>> GetAsync(Expression<Func<T, bool>>? predicate = null, ...);
Task AddAsync(T entity);
void Update(T entity);
void Remove(T entity);
Task<int> SaveChangesAsync();
```

### 2. 认证服务 `AuthService`

**位置**：`Services/AuthService.cs`

**功能**：
- `RegisterAsync`：用户名/邮箱唯一性校验 + BCrypt 哈希
- `ValidateUserAsync`：支持"用户名或邮箱"登录 + 密码验证

---

## 运行截图（需补充）

### 1. 首页
![首页](docs/screenshots/home.png)

### 2. 文章列表
![列表](docs/screenshots/posts-list.png)

### 3. 文章详情
![详情](docs/screenshots/post-detail.png)

### 4. 个人中心
![个人中心](docs/screenshots/profile.png)

---

## 开发团队与分工（示例）

| 成员   | 角色                     | 主要工作                          |
|--------|--------------------------|-----------------------------------|
| 张三   | 后端开发 + 数据库设计    | 实体模型、仓储层、控制器逻辑      |
| 李四   | 前端开发 + UI 设计       | Razor 视图、CSS 美化、响应式布局  |
| 王五   | 全栈 + DevOps            | 图片上传、AJAX交互、部署脚本      |

---

## 部署方式

### 方式 1：本地开发运行
```bash
dotnet run --project .\KnowledgeStack.Web\KnowledgeStack.Web.csproj --urls http://localhost:5080
```

### 方式 2：IIS 公网部署（推荐）

**一键部署**：
```powershell
# 以管理员身份运行
.\deploy-to-iis.ps1 -SqlPassword "你的SQL密码"
```

**手动部署**：参考 `docs/公网部署完整指南.md`

部署后访问：
- 本机：http://localhost
- 公网：http://你的公网IP 或 http://你的域名.com

**详细文档**：
- 📖 `docs/立即部署-执行手册.md` - 30分钟快速部署
- 📖 `docs/公网部署完整指南.md` - 分步详细说明
- 📖 `docs/环境变量配置说明.md` - 安全配置
- 📖 `docs/路由器端口转发配置.md` - 网络配置

### 方式 3：云服务器部署
1. **阿里云/腾讯云**：购买服务器，安装 IIS + SQL Server
2. **Docker 容器**：使用 Dockerfile 构建镜像
3. **Azure App Service**：直接发布，自动配置

---

## 常见问题

### Q1: OFFSET 语法错误？
**A**：数据库兼容级别需 ≥ 110（SQL Server 2012+）
```sql
ALTER DATABASE [KnowledgeStackDb] SET COMPATIBILITY_LEVEL = 150;
```

### Q2: 图片上传失败？
**A**：确保 `wwwroot/uploads/` 目录存在且有写权限

### Q3: 启动后无法访问？
**A**：
1. 检查 SQL Server 实例是否启动
2. 确认端口未被占用（改用 `--urls http://localhost:5000`）
3. 关闭 VPN/代理

---

## 技术亮点

1. ✨ **自定义仓储模式**：解耦数据访问，提升可测试性
2. ✨ **Cookie 认证**：Session 保持，安全登录
3. ✨ **Markdown 支持**：Markdig 渲染，所见即所得
4. ✨ **图片上传**：防 XSS，文件类型/大小验证
5. ✨ **响应式设计**：Bootstrap 5，移动端友好
6. ✨ **AJAX 交互**：点赞无刷新，提升用户体验

---

## 许可证

本项目为 ASP.NET Web 编程课程综合训练作业，仅供学习交流使用。

---

## 联系方式

项目路演与演示视频请见学习通提交记录。

