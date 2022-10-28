# ChinesePinyinIntelliSenseExtender

VisualStudio中文代码拼音补全拓展。

- 仅在 `C#` 和 `F#` 下进行了测试，理论上支持所有基于 `IAsyncCompletionSource` 的完成。

View at [VisualStudio Marketplace](https://marketplace.visualstudio.com/items?itemName=stratos.ChinesePinyinIntelliSenseExtender)

## 效果

![example.gif](./assets/example.gif)

内置的拼音字典支持多音字和繁体字：

![多音字](./assets/duoyinzi.gif)
![繁体字](./assets/fantizi.gif)

可以切换成五笔：

![五笔](./assets/wubi.gif)

或者其它自定义字典（如下图演示的[超强快码](https://github.com/whjiang/cqeb)）：

![自定义字典](./assets/custom_dict.gif)

## 自定义字典

字典是一个 tsv 文件，其内的每一行包括要转换的字和对应的拼写方式，如：

``` tsv
芳	fang
防	fang
房	fang
方	fang
放	fang
```

参考： [pinyin.tsv](./assets/tables/pinyin.tsv) 和 [wubi86.tsv](./assets/tables/wubi86.tsv.tsv)。

[该脚本](./assets/tables/make_word_table_from_rime_dict.fsx)演示了如何将 rime 输入方案的词典文件内取出里面的所有的字。

## 引用

内置的字典来源于下列输入方案的词典：

`assets/tables/pinyin_simp.dict.yaml` - [Rime/rime-pinyin-simp 袖珍简化字拼音](https://github.com/rime/rime-pinyin-simp)（[Apache-2.0 协议](https://github.com/rime/rime-pinyin-simp/blob/master/LICENSE)）

`assets/tables/wubi86_jidian.dict.yaml` - [KyleBing/rime-wubi86-jidian 86五笔极点码表 for 鼠须管(macOS)、小狼毫(Windows)、中州韵(Linux:Ubuntu) 五笔输入法](https://github.com/KyleBing/rime-wubi86-jidian)（[Apache-2.0 协议](https://github.com/KyleBing/rime-wubi86-jidian/blob/master/LICENSE)）
