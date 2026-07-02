MODULE Lib_24_case_L

    VAR string end:="";

    PROC p_case_1_L()
        p_rotate_180_sub_L;
        WaitSyncTask\InPos,sync108,task_list;
        WaitSyncTask\InPos,sync109,task_list;
        WaitTime\InPos,0;
        open_gripper_L;
        AccSet 100,100,\FinePointRamp:=100;
        MoveL TG_case_8,v800,z5,Servo\WObj:=wobj0;
        MoveJ waitpoint,v800,z100,Servo\WObj:=wobj0;
        WaitSyncTask\InPos,sync110,task_list;
        WaitSyncTask\InPos,sync111,task_list;
        AccSet 100,100,\FinePointRamp:=100;
        MoveL center,v800,z5,Servo\WObj:=wobj0;
        close_gripper_L;
        WaitSyncTask\InPos,sync112,task_list;
        WaitSyncTask\InPos,sync196,task_list;
        end:="END1";
    ENDPROC

    PROC p_case_2_L()
        p_rotate_90_sub_L;
        WaitSyncTask\InPos,sync78,task_list;
        WaitTime\InPos,0;
        open_gripper_L;
        AccSet 100,100,\FinePointRamp:=100;
        MoveL TG_case_2,v800,z5,Servo\WObj:=wobj0;
        WaitSyncTask\InPos,sync79,task_list;
        AccSet 100,100,\FinePointRamp:=100;
        MoveJ waitpoint,v800,z100,Servo\WObj:=wobj0;
        WaitSyncTask\InPos,sync80,task_list;
        AccSet 100,100,\FinePointRamp:=100;
        MoveL center,v800,z5,Servo\WObj:=wobj0;
        close_gripper_L;
        WaitSyncTask\InPos,sync81,task_list;
        WaitSyncTask\InPos,sync82,task_list;
        end:="END2";
    ENDPROC

    PROC p_case_3_L()
        WaitSyncTask\InPos,sync83,task_list;
        WaitTime\InPos,0;
        open_gripper_L;
        AccSet 100,100,\FinePointRamp:=100;
        MoveL waitpoint,v800,z5,Servo\WObj:=wobj0;
        WaitSyncTask\InPos,sync84,task_list;
        WaitSyncTask\InPos,sync85,task_list;
        AccSet 100,100,\FinePointRamp:=100;
        MoveL center,v800,z5,Servo\WObj:=wobj0;
        close_gripper_L;
        WaitSyncTask\InPos,sync86,task_list;
        WaitSyncTask\InPos,sync87,task_list;
        end:="END3";
    ENDPROC

    PROC p_case_4_L()
        p_rotate_90_add_L;
        WaitSyncTask\InPos,sync88,task_list;
        WaitTime\InPos,0;
        open_gripper_L;
        AccSet 100,100,\FinePointRamp:=100;
        MoveL TG_case_4,v800,z5,Servo\WObj:=wobj0;
        WaitSyncTask\InPos,sync89,task_list;
        AccSet 100,100,\FinePointRamp:=100;
        MoveJ waitpoint,v800,z100,Servo\WObj:=wobj0;
        WaitSyncTask\InPos,sync90,task_list;
        AccSet 100,100,\FinePointRamp:=100;
        MoveL center,v800,z5,Servo\WObj:=wobj0;
        close_gripper_L;
        WaitSyncTask\InPos,sync91,task_list;
        WaitSyncTask\InPos,sync92,task_list;
        end:="END4";
    ENDPROC

    PROC p_case_5_L()
        p_rotate_90_sub_L;
        AccSet 100,100,\FinePointRamp:=100;
        MoveJ TG_case_5,v800,z100,Servo\WObj:=wobj0;
        WaitSyncTask\InPos,sync94,task_list;
        WaitSyncTask\InPos,sync95,task_list;
        WaitTime\InPos,0;
        open_gripper_L;
        AccSet 100,100,\FinePointRamp:=100;
        MoveL TG_case_5_1,v800,z5,Servo\WObj:=wobj0;
        MoveL TG_case_5_2,v800,z5,Servo\WObj:=wobj0;
        MoveJ waitpoint,v800,z100,Servo\WObj:=wobj0;
        MoveL center,v800,z5,Servo\WObj:=wobj0;
        close_gripper_L;
        WaitSyncTask\InPos,sync96,task_list;
        WaitSyncTask\InPos,sync97,task_list;
        end:="END5";
    ENDPROC

    PROC p_case_6_L()
        AccSet 100,100,\FinePointRamp:=100;
        MoveJ TG_case_6,v800,z100,Servo\WObj:=wobj0;
        WaitSyncTask\InPos,sync98,task_list;
        WaitSyncTask\InPos,sync99,task_list;
        WaitTime\InPos,0;
        open_gripper_L;
        AccSet 100,100,\FinePointRamp:=100;
        MoveL TG_case_6_1,v800,z5,Servo\WObj:=wobj0;
        MoveL TG_case_6_2,v800,z5,Servo\WObj:=wobj0;
        MoveJ waitpoint,v800,z100,Servo\WObj:=wobj0;
        MoveL center,v800,z5,Servo\WObj:=wobj0;
        close_gripper_L;
        WaitSyncTask\InPos,sync100,task_list;
        WaitSyncTask\InPos,sync101,task_list;
        end:="END6";
    ENDPROC

    PROC p_case_7_L()
        AccSet 100,100,\FinePointRamp:=100;
        MoveJ TG_case_6,v800,z100,Servo\WObj:=wobj0;
        WaitSyncTask\InPos,sync102,task_list;
        WaitSyncTask\InPos,sync103,task_list;
        WaitTime\InPos,0;
        open_gripper_L;
        AccSet 100,100,\FinePointRamp:=100;
        MoveL TG_case_6_1,v800,z5,Servo\WObj:=wobj0;
        MoveL TG_case_6_2,v800,z5,Servo\WObj:=wobj0;
        WaitSyncTask\InPos,sync104,task_list;
        AccSet 100,100,\FinePointRamp:=100;
        MoveJ waitpoint,v800,z100,Servo\WObj:=wobj0;
        WaitSyncTask\InPos,sync105,task_list;
        AccSet 100,100,\FinePointRamp:=100;
        MoveL center,v800,z5,Servo\WObj:=wobj0;
        close_gripper_L;
        WaitSyncTask\InPos,sync106,task_list;
        WaitSyncTask\InPos,sync107,task_list;
        end:="END7";
    ENDPROC

    PROC p_case_8_L()
        p_rotate_180_sub_L;
        WaitSyncTask\InPos,sync113,task_list;
        WaitSyncTask\InPos,sync114,task_list;
        WaitTime\InPos,0;
        open_gripper_L;
        AccSet 100,100,\FinePointRamp:=100;
        MoveL TG_case_8,v800,z5,Servo\WObj:=wobj0;
        MoveJ waitpoint,v800,z100,Servo\WObj:=wobj0;
        WaitSyncTask\InPos,sync115,task_list;
        WaitSyncTask\InPos,sync116,task_list;
        AccSet 100,100,\FinePointRamp:=100;
        MoveL center,v800,z5,Servo\WObj:=wobj0;
        close_gripper_L;
        WaitSyncTask\InPos,sync117,task_list;
        WaitSyncTask\InPos,sync118,task_list;
        end:="END8";
    ENDPROC

    PROC p_case_9_L()
        AccSet 100,100,\FinePointRamp:=100;
        MoveJ rotate_D,v800,z100,Servo\WObj:=wobj0;
        WaitSyncTask\InPos,sync119,task_list;
        WaitSyncTask\InPos,sync120,task_list;
        WaitTime\InPos,0;
        open_gripper_L;
        AccSet 100,100,\FinePointRamp:=100;
        MoveL TG_case_9,v800,z5,Servo\WObj:=wobj0;
        MoveL TG_case_9_1,v800,z5,Servo\WObj:=wobj0;
        MoveJ waitpoint,v800,z100,Servo\WObj:=wobj0;
        MoveL center,v800,z5,Servo\WObj:=wobj0;
        close_gripper_L;
        WaitSyncTask\InPos,sync121,task_list;
        WaitSyncTask\InPos,sync122,task_list;
        end:="END9";
    ENDPROC

    PROC p_case_10_L()
        AccSet 100,100,\FinePointRamp:=100;
        MoveJ TG_case_10,v800,z100,Servo\WObj:=wobj0;
        WaitSyncTask\InPos,sync123,task_list;
        WaitSyncTask\InPos,sync124,task_list;
        WaitTime\InPos,0;
        open_gripper_L;
        AccSet 100,100,\FinePointRamp:=100;
        MoveL TG_case_10_1,v800,z5,Servo\WObj:=wobj0;
        MoveL TG_case_10_2,v800,z5,Servo\WObj:=wobj0;
        MoveJ waitpoint,v800,z100,Servo\WObj:=wobj0;
        MoveL center,v800,z5,Servo\WObj:=wobj0;
        close_gripper_L;
        WaitSyncTask\InPos,sync125,task_list;
        WaitSyncTask\InPos,sync126,task_list;
        end:="END10";
    ENDPROC

    PROC p_case_11_L()
        AccSet 100,100,\FinePointRamp:=100;
        MoveJ rotate_U,v800,z100,Servo\WObj:=wobj0;
        WaitSyncTask\InPos,sync127,task_list;
        WaitSyncTask\InPos,sync128,task_list;
        WaitTime\InPos,0;
        open_gripper_L;
        AccSet 100,100,\FinePointRamp:=100;
        MoveL TG_case_11_1,v800,z5,Servo\WObj:=wobj0;
        MoveL TG_case_11_2,v800,z5,Servo\WObj:=wobj0;
        MoveJ waitpoint,v800,z100,Servo\WObj:=wobj0;
        MoveL center,v800,z5,Servo\WObj:=wobj0;
        close_gripper_L;
        WaitSyncTask\InPos,sync129,task_list;
        WaitSyncTask\InPos,sync130,task_list;
        end:="END11";
    ENDPROC

    PROC p_case_12_L()
        AccSet 100,100,\FinePointRamp:=100;
        MoveJ TG_case_12,v800,z100,Servo\WObj:=wobj0;
        WaitSyncTask\InPos,sync131,task_list;
        WaitSyncTask\InPos,sync132,task_list;
        WaitTime\InPos,0;
        open_gripper_L;
        AccSet 100,100,\FinePointRamp:=100;
        MoveL TG_case_12_1,v800,z5,Servo\WObj:=wobj0;
        MoveL TG_case_12_2,v800,z5,Servo\WObj:=wobj0;
        MoveJ waitpoint,v800,z100,Servo\WObj:=wobj0;
        MoveL center,v800,z5,Servo\WObj:=wobj0;
        close_gripper_L;
        WaitSyncTask\InPos,sync133,task_list;
        WaitSyncTask\InPos,sync134,task_list;
        end:="END12";
    ENDPROC

    PROC p_case_13_L()
        AccSet 100,100,\FinePointRamp:=100;
        MoveJ TG_case_5,v800,z100,Servo\WObj:=wobj0;
        WaitSyncTask\InPos,sync135,task_list;
        WaitSyncTask\InPos,sync136,task_list;
        WaitTime\InPos,0;
        open_gripper_L;
        AccSet 100,100,\FinePointRamp:=100;
        MoveL TG_case_5_1,v800,z5,Servo\WObj:=wobj0;
        MoveL TG_case_5_2,v800,z5,Servo\WObj:=wobj0;
        MoveJ waitpoint,v800,z100,Servo\WObj:=wobj0;
        WaitSyncTask\InPos,sync137,task_list;
        WaitSyncTask\InPos,sync138,task_list;
        AccSet 100,100,\FinePointRamp:=100;
        MoveL center,v800,z5,Servo\WObj:=wobj0;
        close_gripper_L;
        WaitSyncTask\InPos,sync139,task_list;
        WaitSyncTask\InPos,sync197,task_list;
        end:="END13";
    ENDPROC

    PROC p_case_14_L()
        p_rotate_90_sub_L;
        WaitSyncTask\InPos,sync140,task_list;
        WaitSyncTask\InPos,sync141,task_list;
        WaitTime\InPos,0;
        open_gripper_L;
        AccSet 100,100,\FinePointRamp:=100;
        MoveL TG_rotate_CCW,v800,z5,Servo\WObj:=wobj0;
        MoveJ waitpoint,v800,z100,Servo\WObj:=wobj0;
        WaitSyncTask\InPos,sync142,task_list;
        WaitSyncTask\InPos,sync143,task_list;
        AccSet 100,100,\FinePointRamp:=100;
        MoveL center,v800,z5,Servo\WObj:=wobj0;
        close_gripper_L;
        WaitSyncTask\InPos,sync144,task_list;
        WaitSyncTask\InPos,sync145,task_list;
        end:="END14";
    ENDPROC

    PROC p_case_15_L()
        AccSet 100,100,\FinePointRamp:=100;
        MoveJ rotate_U,v800,z100,Servo\WObj:=wobj0;
        WaitSyncTask\InPos,sync146,task_list;
        WaitSyncTask\InPos,sync147,task_list;
        WaitTime\InPos,0;
        open_gripper_L;
        AccSet 100,100,\FinePointRamp:=100;
        MoveL TG_case_11_1,v800,z5,Servo\WObj:=wobj0;
        MoveL TG_case_11_2,v800,z5,Servo\WObj:=wobj0;
        MoveJ waitpoint,v800,z100,Servo\WObj:=wobj0;
        WaitSyncTask\InPos,sync148,task_list;
        WaitSyncTask\InPos,sync149,task_list;
        AccSet 100,100,\FinePointRamp:=100;
        MoveL center,v800,z5,Servo\WObj:=wobj0;
        close_gripper_L;
        WaitSyncTask\InPos,sync150,task_list;
        WaitSyncTask\InPos,sync151,task_list;
        end:="END15";
    ENDPROC

    PROC p_case_16_L()
        AccSet 100,100,\FinePointRamp:=100;
        MoveJ TG_case_12,v800,z100,Servo\WObj:=wobj0;
        WaitSyncTask\InPos,sync152,task_list;
        WaitSyncTask\InPos,sync153,task_list;
        WaitTime\InPos,0;
        open_gripper_L;
        AccSet 100,100,\FinePointRamp:=100;
        MoveL TG_case_12_1,v800,z5,Servo\WObj:=wobj0;
        MoveL TG_case_12_2,v800,z5,Servo\WObj:=wobj0;
        MoveJ waitpoint,v800,z100,Servo\WObj:=wobj0;
        WaitSyncTask\InPos,sync154,task_list;
        WaitSyncTask\InPos,sync155,task_list;
        AccSet 100,100,\FinePointRamp:=100;
        MoveL center,v800,z5,Servo\WObj:=wobj0;
        close_gripper_L;
        WaitSyncTask\InPos,sync156,task_list;
        WaitSyncTask\InPos,sync157,task_list;
        end:="END16";
    ENDPROC

    PROC p_case_17_L()
        AccSet 100,100,\FinePointRamp:=100;
        MoveJ rotate_D,v800,z100,Servo\WObj:=wobj0;
        WaitSyncTask\InPos,sync158,task_list;
        WaitSyncTask\InPos,sync159,task_list;
        WaitTime\InPos,0;
        open_gripper_L;
        AccSet 100,100,\FinePointRamp:=100;
        MoveL TG_case_9,v800,z5,Servo\WObj:=wobj0;
        MoveL TG_case_9_1,v800,z5,Servo\WObj:=wobj0;
        WaitSyncTask\InPos,sync160,task_list;
        AccSet 100,100,\FinePointRamp:=100;
        MoveJ waitpoint,v800,z100,Servo\WObj:=wobj0;
        WaitSyncTask\InPos,sync161,task_list;
        AccSet 100,100,\FinePointRamp:=100;
        MoveL center,v800,z5,Servo\WObj:=wobj0;
        close_gripper_L;
        WaitSyncTask\InPos,sync162,task_list;
        WaitSyncTask\InPos,sync163,task_list;
        end:="END17";
    ENDPROC

    PROC p_case_18_L()
        AccSet 100,100,\FinePointRamp:=100;
        MoveJ TG_case_10,v800,z100,Servo\WObj:=wobj0;
        WaitSyncTask\InPos,sync164,task_list;
        WaitSyncTask\InPos,sync165,task_list;
        WaitTime\InPos,0;
        open_gripper_L;
        AccSet 100,100,\FinePointRamp:=100;
        MoveL TG_case_10_1,v800,z5,Servo\WObj:=wobj0;
        MoveL TG_case_10_2,v800,z5,Servo\WObj:=wobj0;
        WaitSyncTask\InPos,sync166,task_list;
        AccSet 100,100,\FinePointRamp:=100;
        MoveJ waitpoint,v800,z100,Servo\WObj:=wobj0;
        WaitSyncTask\InPos,sync167,task_list;
        AccSet 100,100,\FinePointRamp:=100;
        MoveL center,v800,z5,Servo\WObj:=wobj0;
        close_gripper_L;
        WaitSyncTask\InPos,sync168,task_list;
        WaitSyncTask\InPos,sync169,task_list;
        end:="END18";
    ENDPROC

    PROC p_case_19_L()
        AccSet 100,100,\FinePointRamp:=100;
        MoveJ rotate_U,v800,z100,Servo\WObj:=wobj0;
        WaitSyncTask\InPos,sync170,task_list;
        WaitSyncTask\InPos,sync171,task_list;
        WaitTime\InPos,0;
        open_gripper_L;
        AccSet 100,100,\FinePointRamp:=100;
        MoveL TG_case_19_1,v800,z5,Servo\WObj:=wobj0;
        MoveL TG_case_19_2,v800,z5,Servo\WObj:=wobj0;
        WaitSyncTask\InPos,sync172,task_list;
        AccSet 100,100,\FinePointRamp:=100;
        MoveJ waitpoint,v800,z100,Servo\WObj:=wobj0;
        WaitSyncTask\InPos,sync173,task_list;
        AccSet 100,100,\FinePointRamp:=100;
        MoveL center,v800,z5,Servo\WObj:=wobj0;
        close_gripper_L;
        WaitSyncTask\InPos,sync174,task_list;
        WaitSyncTask\InPos,sync175,task_list;
        end:="END19";
    ENDPROC

    PROC p_case_20_L()
        AccSet 100,100,\FinePointRamp:=100;
        MoveJ TG_case_12,v800,z100,Servo\WObj:=wobj0;
        WaitSyncTask\InPos,sync176,task_list;
        WaitSyncTask\InPos,sync177,task_list;
        WaitTime\InPos,0;
        open_gripper_L;
        AccSet 100,100,\FinePointRamp:=100;
        MoveL TG_case_12_1,v800,z5,Servo\WObj:=wobj0;
        MoveL TG_case_12_2,v800,z5,Servo\WObj:=wobj0;
        WaitSyncTask\InPos,sync178,task_list;
        AccSet 100,100,\FinePointRamp:=100;
        MoveJ waitpoint,v800,z100,Servo\WObj:=wobj0;
        WaitSyncTask\InPos,sync179,task_list;
        AccSet 100,100,\FinePointRamp:=100;
        MoveL center,v800,z5,Servo\WObj:=wobj0;
        close_gripper_L;
        WaitSyncTask\InPos,sync180,task_list;
        WaitSyncTask\InPos,sync181,task_list;
        end:="END20";
    ENDPROC

    PROC p_case_21_L()
        p_rotate_180_sub_L;
        WaitSyncTask\InPos,sync182,task_list;
        WaitSyncTask\InPos,sync183,task_list;
        WaitTime\InPos,0;
        open_gripper_L;
        AccSet 100,100,\FinePointRamp:=100;
        MoveL TG_case_21,v800,z5,Servo\WObj:=wobj0;
        MoveJ waitpoint,v800,z100,Servo\WObj:=wobj0;
        MoveL center,v800,z5,Servo\WObj:=wobj0;
        close_gripper_L;
        WaitSyncTask\InPos,sync184,task_list;
        end:="END21";
    ENDPROC

    PROC p_case_22_L()
        WaitSyncTask\InPos,sync185,task_list;
        WaitTime\InPos,0;
        open_gripper_L;
        AccSet 100,100,\FinePointRamp:=100;
        MoveL waitpoint,v800,z5,Servo\WObj:=wobj0;
        WaitSyncTask\InPos,sync186,task_list;
        WaitSyncTask\InPos,sync187,task_list;
        AccSet 100,100,\FinePointRamp:=100;
        MoveL center,v800,z5,Servo\WObj:=wobj0;
        close_gripper_L;
        WaitSyncTask\InPos,sync188,task_list;
        WaitSyncTask\InPos,sync189,task_list;
        end:="END22";
    ENDPROC

    PROC p_case_23_L()
        WaitSyncTask\InPos,sync190,task_list;
        end:="END23";
    ENDPROC

    PROC p_case_24_L()
        WaitSyncTask\InPos,sync191,task_list;
        WaitTime\InPos,0;
        open_gripper_L;
        AccSet 100,100,\FinePointRamp:=100;
        MoveL waitpoint,v800,z5,Servo\WObj:=wobj0;
        WaitSyncTask\InPos,sync192,task_list;
        WaitSyncTask\InPos,sync193,task_list;
        AccSet 100,100,\FinePointRamp:=100;
        MoveL center,v800,z5,Servo\WObj:=wobj0;
        close_gripper_L;
        WaitSyncTask\InPos,sync194,task_list;
        WaitSyncTask\InPos,sync195,task_list;
        end:="END24";
    ENDPROC

    PROC p_24_case_L()

    ENDPROC
ENDMODULE