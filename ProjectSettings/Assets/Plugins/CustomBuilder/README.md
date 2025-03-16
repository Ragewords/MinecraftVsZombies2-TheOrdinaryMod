# Custom Builder

自定义CI构建脚本。

此插件提供了一个`CustomBuilder.Builder.BuildAll()`方法，在执行此方法后便可按照预设的构建配置自动构建相应的项目。构建的项目将会放置在`Build`文件夹下。

此插件提供了`IBuilderStep`接口。任何实现了此接口的类都会在编译时自动调用，从而达到自动更新某些配置文件的效果。。同时，`BuilderStepAttribute`特性可以配置此类的调用顺序。

## 暴露方法

- `CustomBuilder.Builder.BuildAll()` 打包所有配置
- `CustomBuilder.Builder.BuildWindows()` 打包配置中平台为Windows的包
- `CustomBuilder.Builder.BuildAndroid()` 打包配置中平台为Android的包
- `CustomBuilder.Builder.BuildLinux()` 打包配置中平台为Linux的包
- `CustomBuilder.Builder.BuildFromArg()` 从命令行中读取打包平台，然后以默认的配置打包
  - 调用此方法时，会读取命令行中`-buildConfig`这一参数。参数是以分号分割的平台名称，如`win64-debug`。也可以不写后面的Debug或Release包，默认情况下是打Release包的。

## 额外行为

此插件在打包时会以当前时间戳作为安卓的Bundle版本号。

如果当前正在打Debug包，此插件会自动添加“测试版”字样至项目名称后。同时，对应的安卓包名后面也会添加`.test`字样以示区分。

当前打包信息会写入`Assets/Resources/Buildinfo.asset`文件中，其中包括了构建版本和构建日期。构建版本会从`BUILD_GIT_TAG`环境变量中读取，如果没有则尝试读取Git的Tag。如果无法获取到Tag，则默认会使用`0.0.0-gxxxxxxx`这样的git提交hash作为版本号

可以通过环境变量指定安卓打包的签名。环境变量为：

- `KEYSTORE_PATH`
- `KEYSTORE_PASS`
- `KEYALIAS_NAME`
- `KEYALIAS_PASS`

## 更新日志

### 1.11（2021-12-18）

- 新方法`IBuilderStep`代替原有的Hook
- 支持调整构建步骤执行顺序

### 1.10 (2021-11-13)

- 支持自定义打包签名

### 1.9 (2021-11-07)

- 支持本地读取Git Tag

### 1.8 (2021-04-04)

- 安卓的Bundle版本号设置为时间戳

### 1.7  (2021-03-03)

- 支持自定义打包平台了

### 1.6 （2021-02-13）

- 支持打包后的Git Tag

### 1.5 （2021-01-24）

- 修改版本号的获取方式，改为先从环境变量中读取，再读取Git的Tag

### 1.4 （2020-11-15）

- 为测试包添加特殊的包名和名字

### 1.3 （2020-09-28）

- 添加设置安卓Bundle版本号的功能

### 1.2 （2020-08-26）

- 添加单独打包Windows、Android、Linux的方法

### 1.1 （2020-07-18）

- 添加打包信息的写入

### 1.0（2019-04-27）

- 重构成为插件
- 增加了构建配置文件以及对应配置的窗口

### 0.2（2019-03-19）

- 增加了自定义钩子用于加载数据

### 0.1（2018-07-24）

- 初始版本

