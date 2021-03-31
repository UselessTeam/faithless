using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Godot;
using Utils;

public enum YokaiId {
    None,
    Hitotsumekozo,
    Kasaobake,
    Chochinobake,
    Jorogumo,
    TOTAL,
}

public static class YokaiIdExtension {
    public static YokaiData Data (this YokaiId yokai) => YokaiData.YokaiList[yokai];
}