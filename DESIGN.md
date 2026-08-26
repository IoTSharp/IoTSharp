---
name: IoTSharp XJETC Ops Acrylic Control Plane
description: 面向物联网接入、规则与边缘运维的高密度控制台设计系统
colors:
  theme-ocean-dark: "#0E4C7A"
  theme-ocean-light: "#DCECF7"
  theme-pine-dark: "#1E5B4F"
  theme-pine-light: "#DCEEE7"
  theme-steel-dark: "#24566B"
  theme-steel-light: "#DCECF3"
  theme-sage-dark: "#526B55"
  theme-sage-light: "#E3ECD8"
  theme-plum-dark: "#6D4B78"
  theme-plum-light: "#F0E1F2"
  theme-mist-dark: "#3D7180"
  theme-mist-light: "#DCEFF1"
  theme-terracotta-dark: "#8A5A42"
  theme-terracotta-light: "#F3E2D5"
  theme-forest-dark: "#2F5B48"
  theme-forest-light: "#DCECE4"
  theme-cranberry-dark: "#922D42"
  theme-cranberry-light: "#F8DCE2"
  theme-amber-dark: "#8A5D2D"
  theme-amber-light: "#F6E7C8"
  theme-slate-dark: "#3F4D66"
  theme-slate-light: "#E2E7F0"
  theme-rosewood-dark: "#7B3F45"
  theme-rosewood-light: "#F2DFE0"
  ink: "#183B36"
  text: "#23332F"
  text-muted: "#71807C"
  surface: "#FFFFFF"
  page: "#F5FBF8"
  border: "#DFE7E4"
  success: "#157347"
  warning: "#A85D00"
  danger: "#BD3E3E"
  info: "#24566B"
typography:
  display:
    fontFamily: "Segoe UI Variable Display, Segoe UI, PingFang SC, Microsoft YaHei, sans-serif"
    fontSize: "clamp(2rem, 4vw, 3rem)"
    fontWeight: 700
    lineHeight: 1.12
    letterSpacing: "normal"
  body:
    fontFamily: "Segoe UI Variable Text, Segoe UI, PingFang SC, Microsoft YaHei, sans-serif"
    fontSize: "14px"
    fontWeight: 400
    lineHeight: 1.55
    letterSpacing: "normal"
  label:
    fontFamily: "Segoe UI Variable Text, Segoe UI, PingFang SC, Microsoft YaHei, sans-serif"
    fontSize: "12px"
    fontWeight: 600
    lineHeight: 1.35
    letterSpacing: "normal"
rounded:
  small: "4px"
  control: "6px"
  panel: "8px"
  pill: "999px"
spacing:
  page-gutter: "18px"
  topbar: "64px"
  nav-expanded: "220px"
  nav-collapsed: "64px"
components:
  button-primary:
    backgroundColor: "{colors.theme-pine-dark}"
    textColor: "{colors.surface}"
    rounded: "{rounded.control}"
    padding: "10px 18px"
  button-quiet:
    backgroundColor: "{colors.theme-pine-light}"
    textColor: "{colors.ink}"
    rounded: "{rounded.control}"
    padding: "9px 14px"
  input:
    backgroundColor: "{colors.surface}"
    textColor: "{colors.text}"
    rounded: "{rounded.control}"
    padding: "10px 12px"
  card:
    backgroundColor: "{colors.surface}"
    textColor: "{colors.text}"
    rounded: "{rounded.panel}"
    padding: "18px"

## Overview

**Creative North Star: "The Operations Signal Room"**

IoTSharp 的界面把平台当作生产现场的控制平面，而不是营销首页或装饰性仪表盘。深色导航轨承载领域边界，浅色工作区承载对象、状态、版本和审计证据；用户在首屏即可知道当前处于接入采集、实时规则、运维发布还是平台治理。

材质采用 XJETC Ops 的克制亚克力语言：渐变只用于导航、顶栏、工作区引导面和主要动作，表格、表单、状态和长时间阅读区域保持实色与高对比度。12 套主题各自提供一枚深色锚点和一枚浅色陪衬，共 24 个主题核心色，切换只改变品牌表达，不改变状态语义。

**Key Characteristics:**
- 深色 220px 导航轨，64px 半透明顶栏，右侧高密度工作区
- 8px 面板、6px 控件、4px 小元素，避免叠卡和过度圆角
- 明确的 Product、Device、Asset、Gateway、EdgeNode、Collection Template 领域语言
- 语义状态颜色固定为成功、警告、失败、信息四类

## Colors

主题颜色来自 XJETC Ops 主题目录。运行时由 `ClientApp/src/theme/themes.ts` 写入 CSS 变量，默认使用松针绿；深色锚点用于导航和主要动作，浅色陪衬用于选择态和安静引导面。

### Primary
- **动态深色锚点** (`--iotsharp-theme-dark`): 当前主题的主色，承载导航渐变、主要按钮、链接和焦点环。
- **动态浅色陪衬** (`--iotsharp-theme-light`): 当前主题的浅色层，用于选中项、安静背景和主题样本的第二色块。

### Secondary
- **信息蓝** (`#24566B`): 说明性状态、协议提示和非阻塞信息。
- **成功绿** (`#157347`): 在线、已完成、收敛等正向状态。

### Tertiary
- **警告琥珀** (`#A85D00`): 待确认、灰度中、延迟和风险提示。
- **危险红** (`#BD3E3E`): 失败、离线、拒绝和不可逆操作提示。

### Neutral
- **页面底色** (`#F5FBF8`): 工作区背景，随主题由 `--iotsharp-page` 调整。
- **表面白** (`#FFFFFF`): 表格、表单和实体卡片的实色表面。
- **正文** (`#23332F`)、**弱正文** (`#52635F`)、**次要文字** (`#71807C`): 形成清晰的阅读层级。
- **边框** (`#DFE7E4`) 与 **强调边框** (`#C9D5D1`): 轻量分隔，避免重阴影替代结构。

### Named Rules
**The Two-Tone Rule.** 每个主题始终同时展示深色锚点和浅色陪衬；状态色不随主题切换，颜色也不是传达状态的唯一方式。

## Typography

**Display Font:** Segoe UI Variable Display (with Segoe UI, PingFang SC, Microsoft YaHei fallbacks)
**Body Font:** Segoe UI Variable Text (with Segoe UI, PingFang SC, Microsoft YaHei fallbacks)
**Label/Mono Font:** 使用正文族的 12px/600 标签层，不引入独立代码字体。

**Character:** Windows 11 风格的可读、紧凑和中性字形；中文与英文混排保持同一基线，字号差异来自层级而非装饰性字距。

### Hierarchy
- **Display** (700, `clamp(2rem, 4vw, 3rem)`, 1.12): 入口页、登录和初始化页的主标题。
- **Headline** (700, 24-30px, 1.2): 控制台页面标题和关键运行结论。
- **Title** (600, 15-18px, 1.3): 面板标题、导航上下文和表格区标题。
- **Body** (400, 14px, 1.55): 说明、表格内容和操作反馈。
- **Label** (600, 12px, 1.35): 字段名、状态标签和辅助元数据。

### Named Rules
**The Evidence First Rule.** 标题层级先说明对象和状态，再说明操作；不要用大字号制造没有数据支撑的“仪表盘气氛”。

## Layout

控制台采用固定侧栏与可滚动主区：展开侧栏 220px，折叠侧栏 64px，顶栏 64px（移动端 60px），内容最大宽度 1720px，页面水平内边距 18px。顶栏保留当前 section、面包屑/标签页、全局工具和用户入口，导航菜单按接入与采集、实时规则、运维与发布、平台治理分组。

桌面端以两列或多列工作区承载列表、详情、状态和任务证据；移动端在 1000px 以下将侧栏变为抽屉，在 767px 以下把工具区、统计块和详情列改为单列堆叠。表格和表单不使用透明背景，保证连续操作和键盘焦点可见。

## Elevation & Depth

系统使用“色调分层 + 低强度阴影”的混合深度。导航和顶栏通过渐变与边界线建立层次；普通面板使用轻阴影，弹层、主题选择器和抽屉使用更高的浮层阴影与 `backdrop-filter`。毛玻璃只用于顶栏和浮层，不用于数据表格、表单或长文本区域。

### Shadow Vocabulary
- **panel** (`0 8px 22px rgba(24, 45, 39, 0.07)`): 普通工作区面板和实体卡片。
- **float** (`0 18px 42px rgba(18, 37, 31, 0.14)`): 主题选择器、抽屉和入口状态卡。
- **focus** (`0 0 0 3px rgba(var(--iotsharp-accent-rgb), 0.1)`): 键盘或输入焦点的附加环。

### Named Rules
**The Acrylic Boundary Rule.** 亚克力只表达“浮在当前上下文之上”的导航工具；业务事实必须落在不透明表面上。

## Shapes

形状语言偏向紧凑、可扫描的办公工具：面板 8px、控件 6px、小元素 4px；状态和筛选标签使用胶囊形 `999px`，仅在表达状态或选择时使用。边框为 1px 中性线，主要按钮和输入控件不使用夸张圆角；移动端入口 shell 可在窄屏去除外层圆角以贴合视口。

## Components

### Buttons
- **Shape:** 6px 控件半径，10px 纵向内距，主要动作保持稳定高度。
- **Primary:** `--iotsharp-accent-gradient` 或当前主题深色锚点，白色文字；用于进入、保存、发布、确认等明确动作。
- **Hover / Focus:** 主题色加深、轻微阴影和 2px 可见焦点环；减少动态效果时保留状态变化。
- **Secondary / Ghost:** 实色浅背景或透明边框，仅用于返回、筛选、取消和次要入口。

### Chips
- **Style:** `999px` 胶囊，成功/警告/危险/信息使用各自的浅色表面和实色文字。
- **State:** 选中主题、在线状态、灰度状态和过滤条件必须同时提供文字或图标语义。

### Cards / Containers
- **Corner Style:** 工作区面板 8px，入口状态卡 8px；禁止卡片套卡片。
- **Background:** 业务数据使用 `--iotsharp-surface`；引导面使用 `--iotsharp-quiet-gradient`。
- **Shadow Strategy:** 普通面板使用 panel 阴影，浮层使用 float 阴影。
- **Border:** 1px `--iotsharp-border`，状态环可使用语义色。
- **Internal Padding:** 18px 为默认工作区间距，移动端降至 12-14px。

### Inputs / Fields
- **Style:** 白色实色表面、1px 边框、6px 半径，字段标签在控件上方。
- **Focus:** 主题色边框 + 3px 半透明焦点环，错误状态另外使用文字说明和危险色。
- **Error / Disabled:** 错误使用危险色和可读文本；禁用态降低对比但不隐藏字段名。

### Navigation
- **Style:** 深色垂直导航轨使用 `--iotsharp-nav-gradient`，菜单按平台领域分组。
- **Default / Hover / Active:** 默认弱白文字，悬停使用低透明主题层，当前项使用浅色陪衬和主题色文字。
- **Mobile:** 1000px 以下变为带遮罩的抽屉；顶栏保留展开/收起按钮和当前页面上下文。

### Theme Picker

主题选择器以两列列表呈现 12 套主题，每项同时显示深色/浅色圆形样本、主题名称和适用描述；选择后即时写入 CSS 变量并保存在本地存储。

## Do's and Don'ts

### Do:
- **Do** 先把 Product、Device、Asset、Gateway、EdgeNode 和 Collection Template 写进页面标题、导航和详情上下文。
- **Do** 把渐变限制在导航、顶栏、入口引导面和主要动作；让表格、表单、状态和告警保持实色。
- **Do** 用状态文字、图标和语义色三者中的至少两种表达成功、告警、失败、离线和审批状态。
- **Do** 在桌面和移动端都保持 18px/12px 节奏、可见焦点和可关闭的移动侧栏。
- **Do** 让发布任务、配置版本、OTA、回滚和审计在独立运维上下文中呈现，不塞入实时规则链。

### Don't:
- **Don't** 把 Device 当作业务树、发布范围或 Asset 的替代品。
- **Don't** 在数据密集区域使用透明玻璃、渐变文字或低对比度背景。
- **Don't** 用无意义的 eyebrow、装饰性卡片堆叠、侧边色条或系统默认大标题填充空白。
- **Don't** 新增无法由真实 API、权限和审计证据支撑的演示指标或完成状态。
- **Don't** 让主题切换改变成功、警告、失败和信息的语义颜色。
