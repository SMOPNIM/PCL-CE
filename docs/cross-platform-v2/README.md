# PCL CE 跨平台 V2 执行基线

> 状态：已批准执行（有条件通过）
> 基线日期：2026-10-31
> 本目录保存 V2 修订计划的执行基线、Gate 证据和范围变更记录。计划文本本身以本目录为准，聊天记录不作为验收依据。

## 1. 已批准的对外基线

| 项目 | 基线 |
|---|---|
| 对外日历 | 13–15 个月 |
| 对外预算 | 38–42 人月 |
| 是否承诺 9–13 个月 | **否** |
| 4 人熟练团队 | 11–13 个月，36–40 人月 |
| 3 人经验不足 | 15–18 个月，42–48 人月 |
| 当前阶段 | Phase 0（4–5 周） |
| Phase 0 结论 | **未通过，仍在执行中** |

估算口径、WBS 和日历情景见 [estimation-baseline.md](estimation-baseline.md)。

## 2. 阶段与 Gate

| 阶段 | 目标 | 状态 |
|---|---|---|
| Phase 0 | 高风险技术验证、环境准备、基准采样 | **进行中** |
| Phase 1 | Core 去 WPF、平台抽象、DI/组合根、Windows 参考实现 | 未开始（等待 Gate 0） |
| Phase 2 | Avalonia UI 基础层、P0 页面、Mod 管理 Batch 1/2、启动链 | 未开始 |
| Phase 3 | Linux 打包、发行版矩阵、性能与合规收尾 | 未开始 |

Gate 0 的强制项为验证 1、2、3、4、8、9。任一项失败，不进入 Phase 1；先缩范围或调整方案。
此外，Linux IME、原版启动、Forge Linux、AppImage、200 Mod 列表、自定义标题栏中任一项失败，也会触发 Phase 1 暂停评审。

## 3. 变更控制

- 范围冻结后，新增 P0 功能必须走变更评审，并同步更新 WBS、日历和 Gate。
- 管理、QA、评审、集成和等待时间全部进入 WBS，不再以“顺手做掉”处理。
- 所有 Gate 结论必须附带可复现命令、环境信息和原始输出/截图路径。
- 未通过项只能标记为 `BLOCKED`、`FAIL` 或 `PARTIAL`，不能以“基本可用”代替通过。

## 4. 目录

- [Phase 0 执行板](phase-0/README.md)
- [当前代码基线审计](phase-0/baseline-audit.md)
- [Avalonia 12.1.4 验证证据](phase-0/evidence/2026-10-31-avalonia-12.1.4.md)
- [迁移前测试基线](phase-0/evidence/2026-10-31-test-baseline.md)
- [Avalonia 探针工程](../../spikes/Phase0/AvaloniaProbe/README.md)
