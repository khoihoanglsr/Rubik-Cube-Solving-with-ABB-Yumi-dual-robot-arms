MODULE Lib_24_case_R
    VAR string end:="";
    TASK PERS tooldata tGripper:=[TRUE,[[0,0,0],[1,0,0,0]],[0.7,[0,0,0.001],[1,0,0,0],0,0,0]];

    PROC p_case_1_R()
        WaitSyncTask\InPos,sync108,task_list;
        AccSet 100,100,\FinePointRamp:=100;
        MoveL TG_case_1,v800,z5,Servo\WObj:=wobj0;
        close_gripper_R;
        WaitSyncTask\InPos,sync109,task_list;
        WaitSyncTask\InPos,sync110,task_list;
        AccSet 100,100,\FinePointRamp:=100;
        MoveJ TG_case_6,v800,z100,Servo\WObj:=wobj0;
        WaitSyncTask\InPos,sync111,task_list;
        WaitSyncTask\InPos,sync112,task_list;
        open_gripper_R;
        AccSet 100,100,\FinePointRamp:=100;
        MoveL TG_case_6_3,v800,z5,Servo\WObj:=wobj0;
        MoveL TG_case_6_2,v800,z5,Servo\WObj:=wobj0;
        MoveJ TG_case_6_1,v800,z100,Servo\WObj:=wobj0;
        MoveJ waitpoint,v800,z100,Servo\WObj:=wobj0;
        MoveL center,v800,z5,Servo\WObj:=wobj0;
        close_gripper_R;
        WaitSyncTask\InPos,sync196,task_list;
        end:="END1";
    ENDPROC

    PROC p_case_2_R()
        p_rotate_90_sub_R;
        AccSet 100,100,\FinePointRamp:=100;
        MoveL pick_1_point,v800,z5,Servo\WObj:=wobj0;
        close_gripper_R;
        WaitSyncTask\InPos,sync78,task_list;
        WaitSyncTask\InPos,sync79,task_list;
        AccSet 100,100,\FinePointRamp:=100;
        MoveJ TG_case_2_1,v800,z100,Servo\WObj:=wobj0;
        WaitSyncTask\InPos,sync80,task_list;
        WaitSyncTask\InPos,sync81,task_list;
        open_gripper_R;
        AccSet 100,100,\FinePointRamp:=100;
        MoveL TG_case_2_2,v800,z5,Servo\WObj:=wobj0;
        MoveL TG_case_2_3,v800,z5,Servo\WObj:=wobj0;
        MoveJ TG_case_2_4,v800,z100,Servo\WObj:=wobj0;
        MoveL waitpoint,v800,z5,Servo\WObj:=wobj0;
        MoveL center,v800,z5,Servo\WObj:=wobj0;
        close_gripper_R;
        WaitSyncTask\InPos,sync82,task_list;
        end:="END2";
    ENDPROC

    PROC p_case_3_R()
        p_rotate_90_sub_R;
        AccSet 100,100,\FinePointRamp:=100;
        MoveL pick_1_point,v800,z5,Servo\WObj:=wobj0;
        close_gripper_R;
        WaitSyncTask\InPos,sync83,task_list;
        WaitSyncTask\InPos,sync84,task_list;
        AccSet 100,100,\FinePointRamp:=100;
        MoveJ TG_case_2_1,v800,z100,Servo\WObj:=wobj0;
        WaitSyncTask\InPos,sync85,task_list;
        WaitSyncTask\InPos,sync86,task_list;
        open_gripper_R;
        AccSet 100,100,\FinePointRamp:=100;
        MoveL TG_case_2_2,v800,z5,Servo\WObj:=wobj0;
        MoveL TG_case_2_3,v800,z5,Servo\WObj:=wobj0;
        MoveJ TG_case_2_4,v800,z100,Servo\WObj:=wobj0;
        MoveL waitpoint,v800,z5,Servo\WObj:=wobj0;
        MoveL center,v800,z5,Servo\WObj:=wobj0;
        close_gripper_R;
        WaitSyncTask\InPos,sync87,task_list;
        end:="END3";
    ENDPROC

    PROC p_case_4_R()
        p_rotate_90_sub_R;
        AccSet 100,100,\FinePointRamp:=100;
        MoveL pick_1_point,v800,z5,Servo\WObj:=wobj0;
        close_gripper_R;
        WaitSyncTask\InPos,sync88,task_list;
        WaitSyncTask\InPos,sync89,task_list;
        AccSet 100,100,\FinePointRamp:=100;
        MoveJ TG_case_2_1,v800,z100,Servo\WObj:=wobj0;
        WaitSyncTask\InPos,sync90,task_list;
        WaitSyncTask\InPos,sync91,task_list;
        open_gripper_R;
        AccSet 100,100,\FinePointRamp:=100;
        MoveL TG_case_2_2,v800,z5,Servo\WObj:=wobj0;
        MoveL TG_case_2_3,v800,z5,Servo\WObj:=wobj0;
        MoveJ TG_case_2_4,v800,z100,Servo\WObj:=wobj0;
        MoveL waitpoint,v800,z5,Servo\WObj:=wobj0;
        MoveL center,v800,z5,Servo\WObj:=wobj0;
        close_gripper_R;
        WaitSyncTask\InPos,sync92,task_list;
        end:="END4";
    ENDPROC

    PROC p_case_5_R()
        WaitSyncTask\InPos,sync94,task_list;
        AccSet 100,100,\FinePointRamp:=100;
        MoveL TG_case_1,v800,z5,Servo\WObj:=wobj0;
        close_gripper_R;
        WaitSyncTask\InPos,sync95,task_list;
        WaitSyncTask\InPos,sync96,task_list;
        open_gripper_R;
        AccSet 100,100,\FinePointRamp:=100;
        MoveL waitpoint,v800,z5,Servo\WObj:=wobj0;
        MoveL center,v800,z5,Servo\WObj:=wobj0;
        close_gripper_R;
        WaitSyncTask\InPos,sync97,task_list;
        end:="END5";
    ENDPROC

    PROC p_case_6_R()
        WaitSyncTask\InPos,sync98,task_list;
        AccSet 100,100,\FinePointRamp:=100;
        MoveJ TG_case_6_1,v800,z100,Servo\WObj:=wobj0;
        MoveJ TG_case_6_2,v800,z100,Servo\WObj:=wobj0;
        MoveL TG_case_6_3,v800,z5,Servo\WObj:=wobj0;
        MoveL TG_case_6,v800,z5,Servo\WObj:=wobj0;
        close_gripper_R;
        WaitSyncTask\InPos,sync99,task_list;
        WaitSyncTask\InPos,sync100,task_list;
        open_gripper_R;
        AccSet 100,100,\FinePointRamp:=100;
        MoveL TG_case_6_3,v800,z5,Servo\WObj:=wobj0;
        MoveL TG_case_6_2,v800,z5,Servo\WObj:=wobj0;
        MoveJ TG_case_6_1,v800,z100,Servo\WObj:=wobj0;
        MoveJ waitpoint,v800,z100,Servo\WObj:=wobj0;
        MoveL center,v800,z5,Servo\WObj:=wobj0;
        close_gripper_R;
        WaitSyncTask\InPos,sync101,task_list;
        end:="END6";
    ENDPROC

    PROC p_case_7_R()
        WaitSyncTask\InPos,sync102,task_list;
        AccSet 100,100,\FinePointRamp:=100;
        MoveJ TG_case_6_1,v800,z100,Servo\WObj:=wobj0;
        MoveJ TG_case_6_2,v800,z100,Servo\WObj:=wobj0;
        MoveL TG_case_6_3,v800,z5,Servo\WObj:=wobj0;
        MoveL TG_case_6,v800,z5,Servo\WObj:=wobj0;
        close_gripper_R;
        WaitSyncTask\InPos,sync103,task_list;
        WaitSyncTask\InPos,sync104,task_list;
        AccSet 100,100,\FinePointRamp:=100;
        MoveL TG_case_7_1,v800,z5,Servo\WObj:=wobj0;
        MoveJ TG_case_7,v800,z100,Servo\WObj:=wobj0;
        WaitSyncTask\InPos,sync105,task_list;
        WaitSyncTask\InPos,sync106,task_list;
        open_gripper_R;
        AccSet 100,100,\FinePointRamp:=100;
        MoveL center,v800,z5,Servo\WObj:=wobj0;
        close_gripper_R;
        WaitSyncTask\InPos,sync107,task_list;
        end:="END7";
    ENDPROC

    PROC p_case_8_R()
        WaitSyncTask\InPos,sync113,task_list;
        AccSet 100,100,\FinePointRamp:=100;
        MoveL TG_case_1,v800,z5,Servo\WObj:=wobj0;
        close_gripper_R;
        WaitSyncTask\InPos,sync114,task_list;
        WaitSyncTask\InPos,sync115,task_list;
        AccSet 100,100,\FinePointRamp:=100;
        MoveJ TG_case_8,v800,z100,Servo\WObj:=wobj0;
        WaitSyncTask\InPos,sync116,task_list;
        WaitSyncTask\InPos,sync117,task_list;
        open_gripper_R;
        AccSet 100,100,\FinePointRamp:=100;
        MoveL TG_case_8_1,v800,z5,Servo\WObj:=wobj0;
        MoveL TG_case_8_2,v800,z5,Servo\WObj:=wobj0;
        MoveJ waitpoint,v800,z100,Servo\WObj:=wobj0;
        MoveL center,v800,z5,Servo\WObj:=wobj0;
        close_gripper_R;
        WaitSyncTask\InPos,sync118,task_list;
        end:="END8";
    ENDPROC

    PROC p_case_9_R()
        WaitSyncTask\InPos,sync119,task_list;
        AccSet 100,100,\FinePointRamp:=100;
        MoveL TG_case_1,v800,z5,Servo\WObj:=wobj0;
        close_gripper_R;
        WaitSyncTask\InPos,sync120,task_list;
        WaitSyncTask\InPos,sync121,task_list;
        open_gripper_R;
        WaitTime\InPos,0;
        AccSet 100,100,\FinePointRamp:=100;
        MoveL center,v800,z5,Servo\WObj:=wobj0;
        close_gripper_R;
        WaitSyncTask\InPos,sync122,task_list;
        end:="END9";
    ENDPROC

    PROC p_case_10_R()
        p_rotate_90_sub_R;
        WaitSyncTask\InPos,sync123,task_list;
        AccSet 100,100,\FinePointRamp:=100;
        MoveL pick_1_point,v800,z5,Servo\WObj:=wobj0;
        close_gripper_R;
        WaitSyncTask\InPos,sync124,task_list;
        WaitSyncTask\InPos,sync125,task_list;
        open_gripper_R;
        AccSet 100,100,\FinePointRamp:=100;
        MoveL TG_case_10_1,v800,z5,Servo\WObj:=wobj0;
        MoveJ waitpoint,v800,z100,Servo\WObj:=wobj0;
        MoveL center,v800,z5,Servo\WObj:=wobj0;
        close_gripper_R;
        WaitSyncTask\InPos,sync126,task_list;
        end:="END10";
    ENDPROC

    PROC p_case_11_R()
        p_rotate_90_sub_R;
        WaitSyncTask\InPos,sync127,task_list;
        AccSet 100,100,\FinePointRamp:=100;
        MoveL pick_1_point,v800,z5,Servo\WObj:=wobj0;
        close_gripper_R;
        WaitSyncTask\InPos,sync128,task_list;
        WaitSyncTask\InPos,sync129,task_list;
        open_gripper_R;
        AccSet 100,100,\FinePointRamp:=100;
        MoveL TG_case_10_1,v800,z5,Servo\WObj:=wobj0;
        MoveJ waitpoint,v800,z100,Servo\WObj:=wobj0;
        MoveL center,v800,z5,Servo\WObj:=wobj0;
        close_gripper_R;
        WaitSyncTask\InPos,sync130,task_list;
        end:="END11";
    ENDPROC

    PROC p_case_12_R()
        p_rotate_90_sub_R;
        WaitSyncTask\InPos,sync131,task_list;
        AccSet 100,100,\FinePointRamp:=100;
        MoveL pick_1_point,v800,z5,Servo\WObj:=wobj0;
        close_gripper_R;
        WaitSyncTask\InPos,sync132,task_list;
        WaitSyncTask\InPos,sync133,task_list;
        open_gripper_R;
        AccSet 100,100,\FinePointRamp:=100;
        MoveL TG_case_10_1,v800,z5,Servo\WObj:=wobj0;
        MoveJ waitpoint,v800,z100,Servo\WObj:=wobj0;
        MoveL center,v800,z5,Servo\WObj:=wobj0;
        close_gripper_R;
        WaitSyncTask\InPos,sync134,task_list;
        end:="END12";
    ENDPROC

    PROC p_case_13_R()
        WaitSyncTask\InPos,sync135,task_list;
        AccSet 100,100,\FinePointRamp:=100;
        MoveL TG_case_1,v800,z5,Servo\WObj:=wobj0;
        close_gripper_R;
        WaitSyncTask\InPos,sync136,task_list;
        WaitSyncTask\InPos,sync137,task_list;
        AccSet 100,100,\FinePointRamp:=100;
        MoveJ TG_case_2_1,v800,z100,Servo\WObj:=wobj0;
        WaitSyncTask\InPos,sync138,task_list;
        WaitSyncTask\InPos,sync139,task_list;
        open_gripper_R;
        AccSet 100,100,\FinePointRamp:=100;
        MoveL TG_case_2_2,v800,z5,Servo\WObj:=wobj0;
        MoveL TG_case_2_3,v800,z5,Servo\WObj:=wobj0;
        MoveL TG_case_2_4,v800,z5,Servo\WObj:=wobj0;
        MoveJ waitpoint,v800,z100,Servo\WObj:=wobj0;
        MoveL center,v800,z5,Servo\WObj:=wobj0;
        close_gripper_R;
        WaitSyncTask\InPos,sync197,task_list;
        end:="END13";
    ENDPROC

    PROC p_case_14_R()
        WaitSyncTask\InPos,sync140,task_list;
        AccSet 100,100,\FinePointRamp:=100;
        MoveJ TG_case_8_2,v800,z100,Servo\WObj:=wobj0;
        MoveL TG_case_8_1,v800,z5,Servo\WObj:=wobj0;
        MoveL TG_case_8,v800,z5,Servo\WObj:=wobj0;
        close_gripper_R;
        WaitSyncTask\InPos,sync141,task_list;
        WaitSyncTask\InPos,sync142,task_list;
        AccSet 100,100,\FinePointRamp:=100;
        MoveJ waitpoint,v800,z100,Servo\WObj:=wobj0;
        MoveJ TG_case_1_2,v800,z100,Servo\WObj:=wobj0;
        WaitSyncTask\InPos,sync143,task_list;
        WaitSyncTask\InPos,sync144,task_list;
        open_gripper_R;
        AccSet 100,100,\FinePointRamp:=100;
        MoveL TG_case_1_3,v800,z5,Servo\WObj:=wobj0;
        MoveL TG_case_1_4,v800,z5,Servo\WObj:=wobj0;
        MoveJ waitpoint,v800,z100,Servo\WObj:=wobj0;
        MoveL center,v800,z5,Servo\WObj:=wobj0;
        close_gripper_R;
        WaitSyncTask\InPos,sync145,task_list;
        end:="END14";
    ENDPROC

    PROC p_case_15_R()
        p_rotate_90_sub_R;
        WaitSyncTask\InPos,sync146,task_list;
        AccSet 100,100,\FinePointRamp:=100;
        MoveL pick_1_point,v800,z5,Servo\WObj:=wobj0;
        close_gripper_R;
        WaitSyncTask\InPos,sync147,task_list;
        WaitSyncTask\InPos,sync148,task_list;
        AccSet 100,100,\FinePointRamp:=100;
        MoveJ TG_case_15,v800,z100,Servo\WObj:=wobj0;
        WaitSyncTask\InPos,sync149,task_list;
        WaitSyncTask\InPos,sync150,task_list;
        open_gripper_R;
        AccSet 100,100,\FinePointRamp:=100;
        MoveL TG_case_15_1,v800,z5,Servo\WObj:=wobj0;
        MoveL TG_case_15_2,v800,z5,Servo\WObj:=wobj0;
        MoveJ waitpoint,v800,z100,Servo\WObj:=wobj0;
        MoveL center,v800,z5,Servo\WObj:=wobj0;
        close_gripper_R;
        WaitSyncTask\InPos,sync151,task_list;
        end:="END15";
    ENDPROC

    PROC p_case_16_R()
        p_rotate_90_sub_R;
        WaitSyncTask\InPos,sync152,task_list;
        AccSet 100,100,\FinePointRamp:=100;
        MoveL pick_1_point,v800,z5,Servo\WObj:=wobj0;
        close_gripper_R;
        WaitSyncTask\InPos,sync153,task_list;
        WaitSyncTask\InPos,sync154,task_list;
        AccSet 100,100,\FinePointRamp:=100;
        MoveJ TG_case_16,v800,z100,Servo\WObj:=wobj0;
        WaitSyncTask\InPos,sync155,task_list;
        WaitSyncTask\InPos,sync156,task_list;
        open_gripper_R;
        AccSet 100,100,\FinePointRamp:=100;
        MoveL TG_case_16_1,v800,z5,Servo\WObj:=wobj0;
        MoveL TG_case_16_2,v800,z5,Servo\WObj:=wobj0;
        MoveJ waitpoint,v800,z100,Servo\WObj:=wobj0;
        MoveL center,v800,z5,Servo\WObj:=wobj0;
        close_gripper_R;
        WaitSyncTask\InPos,sync157,task_list;
        end:="END16";
    ENDPROC

    PROC p_case_17_R()
        WaitSyncTask\InPos,sync158,task_list;
        AccSet 100,100,\FinePointRamp:=100;
        MoveL TG_case_1,v800,z5,Servo\WObj:=wobj0;
        close_gripper_R;
        WaitSyncTask\InPos,sync159,task_list;
        WaitSyncTask\InPos,sync160,task_list;
        p_rotate_90_add_R;
        WaitSyncTask\InPos,sync161,task_list;
        WaitSyncTask\InPos,sync162,task_list;
        open_gripper_R;
        AccSet 100,100,\FinePointRamp:=100;
        MoveL TG_case_17,v800,z5,Servo\WObj:=wobj0;
        MoveJ waitpoint,v800,z100,Servo\WObj:=wobj0;
        MoveL center,v800,z5,Servo\WObj:=wobj0;
        close_gripper_R;
        WaitSyncTask\InPos,sync163,task_list;
        end:="END17";
    ENDPROC

    PROC p_case_18_R()
        p_rotate_90_sub_R;
        WaitSyncTask\InPos,sync164,task_list;
        AccSet 100,100,\FinePointRamp:=100;
        MoveL pick_1_point,v800,z5,Servo\WObj:=wobj0;
        close_gripper_R;
        WaitSyncTask\InPos,sync165,task_list;
        WaitSyncTask\InPos,sync166,task_list;
        p_rotate_90_add_R;
        WaitSyncTask\InPos,sync167,task_list;
        WaitSyncTask\InPos,sync168,task_list;
        open_gripper_R;
        AccSet 100,100,\FinePointRamp:=100;
        MoveL waitpoint,v800,z5,Servo\WObj:=wobj0;
        MoveL center,v800,z5,Servo\WObj:=wobj0;
        close_gripper_R;
        WaitSyncTask\InPos,sync169,task_list;
        end:="END18";
    ENDPROC

    PROC p_case_19_R()
        WaitSyncTask\InPos,sync170,task_list;
        AccSet 100,100,\FinePointRamp:=100;
        MoveL TG_case_1,v800,z5,Servo\WObj:=wobj0;
        close_gripper_R;
        WaitSyncTask\InPos,sync171,task_list;
        WaitSyncTask\InPos,sync172,task_list;
        p_rotate_90_add_R;
        WaitSyncTask\InPos,sync173,task_list;
        WaitSyncTask\InPos,sync174,task_list;
        open_gripper_R;
        AccSet 100,100,\FinePointRamp:=100;
        MoveL TG_case_17,v800,z5,Servo\WObj:=wobj0;
        MoveJ waitpoint,v800,z100,Servo\WObj:=wobj0;
        MoveL center,v800,z5,Servo\WObj:=wobj0;
        close_gripper_R;
        WaitSyncTask\InPos,sync175,task_list;
        end:="END19";
    ENDPROC

    PROC p_case_20_R()
        p_rotate_90_sub_R;
        WaitSyncTask\InPos,sync176,task_list;
        AccSet 100,100,\FinePointRamp:=100;
        MoveL pick_1_point,v800,z5,Servo\WObj:=wobj0;
        close_gripper_R;
        WaitSyncTask\InPos,sync177,task_list;
        WaitSyncTask\InPos,sync178,task_list;
        p_rotate_90_add_R;
        WaitSyncTask\InPos,sync179,task_list;
        WaitSyncTask\InPos,sync180,task_list;
        open_gripper_R;
        AccSet 100,100,\FinePointRamp:=100;
        MoveL waitpoint,v800,z5,Servo\WObj:=wobj0;
        MoveL center,v800,z5,Servo\WObj:=wobj0;
        close_gripper_R;
        WaitSyncTask\InPos,sync181,task_list;
        end:="END20";
    ENDPROC

    PROC p_case_21_R()
        WaitSyncTask\InPos,sync182,task_list;
        AccSet 100,100,\FinePointRamp:=100;
        MoveL center,v800,z5,Servo\WObj:=wobj0;
        close_gripper_R;
        WaitSyncTask\InPos,sync183,task_list;
        WaitSyncTask\InPos,sync184,task_list;
        end:="END21";
    ENDPROC

    PROC p_case_22_R()
        AccSet 100,100,\FinePointRamp:=100;
        MoveL TG_case_1,v800,z5,Servo\WObj:=wobj0;
        close_gripper_R;
        WaitSyncTask\InPos,sync185,task_list;
        WaitSyncTask\InPos,sync186,task_list;
        p_rotate_90_add_R;
        WaitSyncTask\InPos,sync187,task_list;
        WaitSyncTask\InPos,sync188,task_list;
        open_gripper_R;
        AccSet 100,100,\FinePointRamp:=100;
        MoveL TG_case_17,v800,z5,Servo\WObj:=wobj0;
        MoveJ waitpoint,v800,z100,Servo\WObj:=wobj0;
        MoveL center,v800,z5,Servo\WObj:=wobj0;
        close_gripper_R;
        WaitSyncTask\InPos,sync189,task_list;
        end:="END22";
    ENDPROC

    PROC p_case_23_R()
        AccSet 100,100,\FinePointRamp:=100;
        MoveL center,v800,z5,Servo\WObj:=wobj0;
        close_gripper_R;
        WaitSyncTask\InPos,sync190,task_list;
        end:="END23";
    ENDPROC

    PROC p_case_24_R()
        AccSet 100,100,\FinePointRamp:=100;
        MoveL TG_case_1,v800,z5,Servo\WObj:=wobj0;
        close_gripper_R;
        WaitSyncTask\InPos,sync191,task_list;
        WaitSyncTask\InPos,sync192,task_list;
        p_rotate_90_sub_R;
        WaitSyncTask\InPos,sync193,task_list;
        WaitSyncTask\InPos,sync194,task_list;
        open_gripper_R;
        AccSet 100,100,\FinePointRamp:=100;
        MoveL TG_case_24,v800,z5,Servo\WObj:=wobj0;
        MoveJ waitpoint,v800,z100,Servo\WObj:=wobj0;
        MoveL center,v800,z5,Servo\WObj:=wobj0;
        close_gripper_R;
        WaitSyncTask\InPos,sync195,task_list;
        end:="END24";
    ENDPROC

    PROC p_24_case_R()

    ENDPROC
ENDMODULE