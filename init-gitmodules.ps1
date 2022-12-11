# git submodule add https://github.com/rime/rime-pinyin-simp ./third/rime-pinyin-simp
# git submodule add https://github.com/KyleBing/rime-wubi86-jidian ./third/rime-wubi86-jidian
# git submodule add https://github.com/biopolyhedron/rime-jap-poly ./third/rime-jap-poly

Remove-Item -Recurse -Force ./third/rime-pinyin-simp
Remove-Item -Recurse -Force ./third/rime-wubi86-jidian
Remove-Item -Recurse -Force ./third/rime-jap-poly

git submodule init
git submodule update

git submodule status