# ChinesePinyinIntelliSenseExtender

VisualStudio中文代码拼音补全拓展。

- 仅在 `C#` 和 `F#` 下进行了测试，理论上支持所有基于 `IAsyncCompletionSource` 的完成。
- 支持自定义字符映射。

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

自定义字符串映射：

![自定义字符串映射](./assets/custom_string_map.png)

## 自定义字典

字典是一个 tsv 文件，其内的每一行包括要映射的字符串和对应的映射值，原字符与映射之间使用 `\t` 来分隔，映射值可以使用空格进行分隔。如：

``` tsv
芳	fang
防	fang
房	fang
方	fang
放	fang
放假	fang jia
放假了	FangJiaLe
```

参考： [pinyin.tsv](./src/Assets/Tables/pinyin.tsv) 和 [wubi86.tsv](./src/Assets/Tables/wubi86.tsv)。

[该脚本](./script/make_word_table_from_rime_dict.fsx)演示了如何将 rime 输入方案的词典文件内取出里面的所有的字。

---------------------

### * 现在支持直接使用 rime 输入方案的词典文件

---------------------

## 引用

内置的字典来源于下列输入方案的词典：

`./src/Assets/Tables/pinyin.tsv` -> `pinyin_simp.dict.yaml` - [Rime/rime-pinyin-simp 袖珍简化字拼音](https://github.com/rime/rime-pinyin-simp)（[Apache-2.0 协议](https://github.com/rime/rime-pinyin-simp/blob/master/LICENSE)）

`./src/Assets/Tables/wubi86.tsv` -> `wubi86_jidian.dict.yaml` - [KyleBing/rime-wubi86-jidian 86五笔极点码表 for 鼠须管(macOS)、小狼毫(Windows)、中州韵(Linux:Ubuntu) 五笔输入法](https://github.com/KyleBing/rime-wubi86-jidian)（[Apache-2.0 协议](https://github.com/KyleBing/rime-wubi86-jidian/blob/master/LICENSE)）

## 其他平台类似插件

* [Jetbrains/IntelliJ](https://github.com/tuchg/ChinesePinyin-CodeCompletionHelper)
* [VSCode](https://gitee.com/Program-in-Chinese/vscode_Chinese_Input_Assistant)
