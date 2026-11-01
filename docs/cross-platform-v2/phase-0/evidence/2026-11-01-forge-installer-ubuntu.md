# 验证 4 进展：Forge Linux 安装器

> 日期：2026-11-01
> 环境：Ubuntu 22.04.5 LTS / x86_64 / OpenJDK 17.0.20
> 结论：**BLOCKED。** Forge 安装器在 TLS 证书检查阶段等待超时；没有生成 Forge 客户端版本。

## 产物和隔离

- 官方 Maven 下载地址：`https://maven.minecraftforge.net/net/minecraftforge/forge/1.20.1-47.4.0/forge-1.20.1-47.4.0-installer.jar`
- 文件位置：`/home/user/phase0-forge-probe-20261101/forge-installer.jar`
- SHA-256：`f3f57465e2cbdc328193f77ba7f0c94cb64405f31ed556753d9275e8bdc06e2c`
- 安装目标：`/home/user/phase0-forge-probe-20261101/client`，不使用或修改用户的 `.minecraft`。

Installer CLI 帮助正常；首次安装因缺少 launcher profile 提示先运行官方 Launcher。随后在隔离目录创建了最小 `launcher_profiles.json` 并重试：

```bash
JAVA_HOME=/usr/lib/jvm/java-17-openjdk-amd64 \
PATH=/usr/lib/jvm/java-17-openjdk-amd64/bin:$PATH \
timeout 600s java -jar forge-installer.jar --installClient \
  /home/user/phase0-forge-probe-20261101/client
```

安装器解析出 Minecraft/Forge 主机并进入网络安装流程，但在 `net.minecraftforge.installer.DownloadUtils.checkCertificate` 的 TLS 握手读取中等待。10 分钟后 `timeout` 以退出码 **124** 结束进程。目标目录只有测试用 `launcher_profiles.json`，未生成 Forge profile/version。

没有禁用 TLS 证书校验或绕过安装器下载验证。需先排查 VM 到 Forge Maven/证书检查端点的 Java TLS 连接，再重跑安装和版本启动。
