MODULE Lib_Rotate_HandL

    VAR string sol:="";
    VAR string number:="";
    VAR string fn:="";
    VAR string next:="";
    TASK PERS tooldata tGripper:=[TRUE,[[0,0,0],[1,0,0,0]],[0.7,[0,0,0.001],[1,0,0,0],0,0,0]];

    PROC p_end_L()
        WaitTime\InPos,0;
        MotionSup \On \TuneValue:=210;
        open_gripper_L;
        AccSet 100,100,\FinePointRamp:=100;
        MoveL waitpoint,v800,z5,Servo\WObj:=wobj0;
        WaitSyncTask\InPos,sync7,task_list;
        p_home_L;
        WaitSyncTask\InPos,sync8,task_list;
    ENDPROC

    PROC p_rotate_R_CCW()
        MotionSup \On \TuneValue:=210;
        open_gripper_L;
        AccSet 100,100,\FinePointRamp:=100;
        MoveL rotate_BFR,v800,z5,Servo\WObj:=wobj0;
        close_gripper_L;
        p_rotate_90_sub_L;
        ! Xoay nghich
        WaitTime\InPos,0;
        open_gripper_L;
        AccSet 100,100,\FinePointRamp:=100;
        MoveL TG_rotate_CCW,v800,z5,Servo\WObj:=wobj0;
        !Chen If...Else
        IF (next="B") OR (next="B'") OR (next="B2") OR (next="F") OR (next="F'") OR (next="F2") THEN
            p_TG_case_RF_RB_hL;
            !        ELSEIF (next="F") OR (next="F'") OR (next="F2") THEN
            !            p_TG_case_RF_RB_hL;
        ELSE
            MoveL waitpoint,v800,z5,Servo\WObj:=wobj0;
            MoveL center,v800,z5,Servo\WObj:=wobj0;
            close_gripper_L;
            WaitSyncTask sync57,task_list;
        ENDIF
        sol:="";
        fn:="FNRN";
        next:="";
    ENDPROC

    PROC p_rotate_R_CW()
        MotionSup \On \TuneValue:=210;
        open_gripper_L;
        AccSet 100,100,\FinePointRamp:=100;
        MoveL rotate_BFR,v800,z5,Servo\WObj:=wobj0;
        close_gripper_L;
        p_rotate_90_add_L;
        ! Xoay thuan
        WaitTime\InPos,0;
        open_gripper_L;
        AccSet 100,100,\FinePointRamp:=100;
        MoveL TG_rotate_CW,v800,z5,Servo\WObj:=wobj0;
        !Chen If...Else
        IF (next="B") OR (next="B'") OR (next="B2") OR (next="F") OR (next="F'") OR (next="F2") THEN
            p_TG_case_RF_RB_hL;
            !        ELSEIF (next="F") OR (next="F'") OR (next="F2") THEN
            !            p_TG_case_RF_RB_hL;
        ELSE
            MoveL waitpoint,v800,z5,Servo\WObj:=wobj0;
            MoveL center,v800,z5,Servo\WObj:=wobj0;
            close_gripper_L;
            WaitSyncTask sync58,task_list;
        ENDIF
        sol:="";
        fn:="FNR";
        next:="";
    ENDPROC

    PROC p_rotate_R_x2()
        MotionSup \On \TuneValue:=210;
        open_gripper_L;
        AccSet 100,100,\FinePointRamp:=100;
        MoveL rotate_BFR,v800,z5,Servo\WObj:=wobj0;
        close_gripper_L;
        p_rotate_180_sub_L;
        ! Xoay 2 lan
        WaitTime\InPos,0;
        open_gripper_L;
        AccSet 100,100,\FinePointRamp:=100;
        MoveL TG_rotate_x2,v800,z5,Servo\WObj:=wobj0;
        !Chen If...Else
        IF (next="B") OR (next="B'") OR (next="B2") OR (next="F") OR (next="F'") OR (next="F2") THEN
            p_TG_case_RF_RB_hL;
            !        ELSEIF (next="F") OR (next="F'") OR (next="F2") THEN
            !            p_TG_case_RF_RB_hL;
        ELSE
            MoveL waitpoint,v800,z5,Servo\WObj:=wobj0;
            MoveL center,v800,z5,Servo\WObj:=wobj0;
            close_gripper_L;
            WaitSyncTask sync59,task_list;
        ENDIF
        sol:="";
        fn:="FNR2";
        next:="";
    ENDPROC

    PROC p_rotate_D_CCW()
        WaitSyncTask sync24,task_list;
        MotionSup \On \TuneValue:=210;
        AccSet 100,100,\FinePointRamp:=100;
        MoveJ rotate_D,v800,z100,Servo\WObj:=wobj0;
        WaitSyncTask\InPos,sync25,task_list;
        !Chen If...Else
        IF (next="U") OR (next="U'") OR (next="U2") THEN
            p_TG_case_UD_DU_hL;
        ELSEIF (next="L") OR (next="L'") OR (next="L2") THEN
            p_TG_case_UL_DL_hL;
        ELSE
            WaitSyncTask sync26,task_list;
            AccSet 100,100,\FinePointRamp:=100;
            MoveJ center,v800,z100,Servo\WObj:=wobj0;
            WaitSyncTask\InPos,sync27,task_list;
            WaitSyncTask sync28,task_list;
        ENDIF
        sol:="";
        fn:="FNDN";
        next:="";
    ENDPROC

    PROC p_rotate_D_CW()
        WaitSyncTask sync29,task_list;
        MotionSup \On \TuneValue:=210;
        AccSet 100,100,\FinePointRamp:=100;
        MoveJ rotate_D,v800,z100,Servo\WObj:=wobj0;
        WaitSyncTask\InPos,sync30,task_list;
        !Chen If...Else
        IF (next="U") OR (next="U'") OR (next="U2") THEN
            p_TG_case_UD_DU_hL;
        ELSEIF (next="L") OR (next="L'") OR (next="L2") THEN
            p_TG_case_UL_DL_hL;
        ELSE
            WaitSyncTask sync31,task_list;
            AccSet 100,100,\FinePointRamp:=100;
            MoveJ center,v800,z100,Servo\WObj:=wobj0;
            WaitSyncTask\InPos,sync32,task_list;
            WaitSyncTask sync33,task_list;
        ENDIF
        sol:="";
        fn:="FND";
        next:="";
    ENDPROC

    PROC p_rotate_D_x2()
        WaitSyncTask sync34,task_list;
        MotionSup \On \TuneValue:=210;
        AccSet 100,100,\FinePointRamp:=100;
        MoveJ rotate_D,v800,z100,Servo\WObj:=wobj0;
        WaitSyncTask\InPos,sync35,task_list;
        !Chen If...Else
        IF (next="U") OR (next="U'") OR (next="U2") THEN
            p_TG_case_UD_DU_hL;
        ELSEIF (next="L") OR (next="L'") OR (next="L2") THEN
            p_TG_case_UL_DL_hL;
        ELSE
            WaitSyncTask sync36,task_list;
            AccSet 100,100,\FinePointRamp:=100;
            MoveJ center,v800,z100,Servo\WObj:=wobj0;
            WaitSyncTask\InPos,sync37,task_list;
            WaitSyncTask sync38,task_list;
        ENDIF
        sol:="";
        fn:="FND2";
        next:="";
    ENDPROC

    PROC p_rotate_U_CCW()
        WaitSyncTask sync60,task_list;
        MotionSup \On \TuneValue:=210;
        AccSet 100,100,\FinePointRamp:=100;
        MoveJ rotate_U,v800,z100,Servo\WObj:=wobj0;
        WaitSyncTask\InPos,sync61,task_list;
        !Chen If...Else
        IF (next="D") OR (next="D'") OR (next="D2") THEN
            p_TG_case_UD_DU_hL;
        ELSEIF (next="L") OR (next="L'") OR (next="L2") THEN
            p_TG_case_UL_DL_hL;
        ELSE
            WaitSyncTask sync62,task_list;
            AccSet 100,100,\FinePointRamp:=100;
            MoveJ center,v800,z100,Servo\WObj:=wobj0;
            WaitSyncTask\InPos,sync63,task_list;
            WaitSyncTask sync64,task_list;
        ENDIF
        sol:="";
        fn:="FNUN";
        next:="";
    ENDPROC

    PROC p_rotate_U_CW()
        WaitSyncTask sync65,task_list;
        MotionSup \On \TuneValue:=210;
        AccSet 100,100,\FinePointRamp:=100;
        MoveJ rotate_U,v800,z100,Servo\WObj:=wobj0;
        WaitSyncTask\InPos,sync66,task_list;
        !Chen If...Else
        IF (next="D") OR (next="D'") OR (next="D2") THEN
            p_TG_case_UD_DU_hL;
        ELSEIF (next="L") OR (next="L'") OR (next="L2") THEN
            p_TG_case_UL_DL_hL;
        ELSE
            WaitSyncTask sync67,task_list;
            AccSet 100,100,\FinePointRamp:=100;
            MoveJ center,v800,z100,Servo\WObj:=wobj0;
            WaitSyncTask\InPos,sync68,task_list;
            WaitSyncTask sync69,task_list;
        ENDIF
        sol:="";
        fn:="FNU";
        next:="";
    ENDPROC

    PROC p_rotate_U_x2()
        WaitSyncTask sync70,task_list;
        MotionSup \On \TuneValue:=210;
        AccSet 100,100,\FinePointRamp:=100;
        MoveJ rotate_U,v800,z100,Servo\WObj:=wobj0;
        WaitSyncTask\InPos,sync71,task_list;
        !Chen If...Else
        IF (next="D") OR (next="D'") OR (next="D2") THEN
            p_TG_case_UD_DU_hL;
        ELSEIF (next="L") OR (next="L'") OR (next="L2") THEN
            p_TG_case_UL_DL_hL;
        ELSE
            WaitSyncTask sync72,task_list;
            AccSet 100,100,\FinePointRamp:=100;
            MoveJ center,v800,z100,Servo\WObj:=wobj0;
            WaitSyncTask\InPos,sync73,task_list;
            WaitSyncTask sync74,task_list;
        ENDIF
        sol:="";
        fn:="FNU2";
        next:="";
    ENDPROC

    PROC p_rotate_B_CCW()
        MotionSup \On \TuneValue:=210;
        open_gripper_L;
        AccSet 100,100,\FinePointRamp:=100;
        MoveL waitpoint,v800,z5,Servo\WObj:=wobj0;
        WaitTime\InPos,0;
        WaitSyncTask sync9,task_list;
        WaitSyncTask\InPos,sync10,task_list;
        AccSet 100,100,\FinePointRamp:=100;
        MoveL rotate_BFR,v800,z5,Servo\WObj:=wobj0;
        close_gripper_L;
        p_rotate_90_sub_L;
        ! Xoay nghich
        WaitTime\InPos,0;
        open_gripper_L;
        AccSet 100,100,\FinePointRamp:=100;
        MoveL TG_rotate_CCW,v800,z5,Servo\WObj:=wobj0;
        !Chen If...Else
        IF (next="F") OR (next="F'") OR (next="F2") THEN
            p_TG_case_BF_FB_hL;
        ELSEIF (next="R") OR (next="R'") OR (next="R2") THEN
            p_TG_case_BR_FR_hL;
        ELSE
            MoveL waitpoint,v800,z5,Servo\WObj:=wobj0;
            WaitSyncTask sync11,task_list;
            WaitSyncTask\InPos,sync12,task_list;
            AccSet 100,100,\FinePointRamp:=100;
            MoveL center,v800,z5,Servo\WObj:=wobj0;
            close_gripper_L;
            WaitSyncTask sync13,task_list;
        ENDIF
        sol:="";
        fn:="FNBN";
        next:="";
    ENDPROC

    PROC p_rotate_B_CW()
        MotionSup \On \TuneValue:=210;
        open_gripper_L;
        AccSet 100,100,\FinePointRamp:=100;
        MoveL waitpoint,v800,z5,Servo\WObj:=wobj0;
        WaitTime\InPos,0;
        WaitSyncTask sync14,task_list;
        WaitSyncTask\InPos,sync15,task_list;
        AccSet 100,100,\FinePointRamp:=100;
        MoveL rotate_BFR,v800,z5,Servo\WObj:=wobj0;
        close_gripper_L;
        p_rotate_90_add_L;
        ! Xoay thuan
        WaitTime\InPos,0;
        open_gripper_L;
        AccSet 100,100,\FinePointRamp:=100;
        MoveL TG_rotate_CW,v800,z5,Servo\WObj:=wobj0;
        !Chen If...Else
        IF (next="F") OR (next="F'") OR (next="F2") THEN
            p_TG_case_BF_FB_hL;
        ELSEIF (next="R") OR (next="R'") OR (next="R2") THEN
            p_TG_case_BR_FR_hL;
        ELSE
            MoveL waitpoint,v800,z5,Servo\WObj:=wobj0;
            WaitSyncTask sync16,task_list;
            WaitSyncTask\InPos,sync17,task_list;
            AccSet 100,100,\FinePointRamp:=100;
            MoveL center,v800,z5,Servo\WObj:=wobj0;
            close_gripper_L;
            WaitSyncTask sync18,task_list;
        ENDIF
        sol:="";
        fn:="FNB";
        next:="";
    ENDPROC

    PROC p_rotate_B_x2()
        MotionSup \On \TuneValue:=210;
        open_gripper_L;
        AccSet 100,100,\FinePointRamp:=100;
        MoveL waitpoint,v800,z5,Servo\WObj:=wobj0;
        WaitTime\InPos,0;
        WaitSyncTask sync19,task_list;
        WaitSyncTask\InPos,sync20,task_list;
        AccSet 100,100,\FinePointRamp:=100;
        MoveL rotate_BFR,v800,z5,Servo\WObj:=wobj0;
        close_gripper_L;
        p_rotate_180_sub_L;
        ! Xoay 2 lan
        WaitTime\InPos,0;
        open_gripper_L;
        AccSet 100,100,\FinePointRamp:=100;
        MoveL TG_rotate_x2,v800,z5,Servo\WObj:=wobj0;
        !Chen If...Else
        IF (next="F") OR (next="F'") OR (next="F2") THEN
            p_TG_case_BF_FB_hL;
        ELSEIF (next="R") OR (next="R'") OR (next="R2") THEN
            p_TG_case_BR_FR_hL;
        ELSE
            MoveL waitpoint,v800,z5,Servo\WObj:=wobj0;
            WaitSyncTask sync21,task_list;
            WaitSyncTask\InPos,sync22,task_list;
            AccSet 100,100,\FinePointRamp:=100;
            MoveL center,v800,z5,Servo\WObj:=wobj0;
            close_gripper_L;
            WaitSyncTask sync23,task_list;
        ENDIF
        sol:="";
        fn:="FNB2";
        next:="";
    ENDPROC

    PROC p_rotate_F_CCW()
        MotionSup \On \TuneValue:=210;
        open_gripper_L;
        AccSet 100,100,\FinePointRamp:=100;
        MoveL waitpoint,v800,z5,Servo\WObj:=wobj0;
        WaitTime\InPos,0;
        WaitSyncTask sync39,task_list;
        WaitSyncTask\InPos,sync40,task_list;
        AccSet 100,100,\FinePointRamp:=100;
        MoveL rotate_BFR,v800,z5,Servo\WObj:=wobj0;
        close_gripper_L;
        p_rotate_90_sub_L;
        ! Xoay nghich
        WaitTime\InPos,0;
        open_gripper_L;
        AccSet 100,100,\FinePointRamp:=100;
        MoveL TG_rotate_CCW,v800,z5,Servo\WObj:=wobj0;
        !Chen If...Else
        IF (next="B") OR (next="B'") OR (next="B2") THEN
            p_TG_case_BF_FB_hL;
        ELSEIF (next="R") OR (next="R'") OR (next="R2") THEN
            p_TG_case_BR_FR_hL;
        ELSE
            MoveL waitpoint,v800,z5,Servo\WObj:=wobj0;
            WaitSyncTask sync41,task_list;
            WaitSyncTask\InPos,sync42,task_list;
            AccSet 100,100,\FinePointRamp:=100;
            MoveL center,v800,z5,Servo\WObj:=wobj0;
            close_gripper_L;
            WaitSyncTask sync43,task_list;
        ENDIF
        sol:="";
        fn:="FNFN";
        next:="";
    ENDPROC

    PROC p_rotate_F_CW()
        MotionSup \On \TuneValue:=210;
        open_gripper_L;
        AccSet 100,100,\FinePointRamp:=100;
        MoveL waitpoint,v800,z5,Servo\WObj:=wobj0;
        WaitTime\InPos,0;
        WaitSyncTask sync44,task_list;
        WaitSyncTask\InPos,sync45,task_list;
        AccSet 100,100,\FinePointRamp:=100;
        MoveL rotate_BFR,v800,z5,Servo\WObj:=wobj0;
        close_gripper_L;
        p_rotate_90_add_L;
        ! Xoay thuan
        WaitTime\InPos,0;
        open_gripper_L;
        AccSet 100,100,\FinePointRamp:=100;
        MoveL TG_rotate_CW,v800,z5,Servo\WObj:=wobj0;
        !Chen If...Else
        IF (next="B") OR (next="B'") OR (next="B2") THEN
            p_TG_case_BF_FB_hL;
        ELSEIF (next="R") OR (next="R'") OR (next="R2") THEN
            p_TG_case_BR_FR_hL;
        ELSE
            MoveL waitpoint,v800,z5,Servo\WObj:=wobj0;
            WaitSyncTask sync46,task_list;
            WaitSyncTask\InPos,sync47,task_list;
            AccSet 100,100,\FinePointRamp:=100;
            MoveL center,v800,z5,Servo\WObj:=wobj0;
            close_gripper_L;
            WaitSyncTask sync48,task_list;
        ENDIF
        sol:="";
        fn:="FNF";
        next:="";
    ENDPROC

    PROC p_rotate_F_x2()
        MotionSup \On \TuneValue:=210;
        open_gripper_L;
        AccSet 100,100,\FinePointRamp:=100;
        MoveL waitpoint,v800,z5,Servo\WObj:=wobj0;
        WaitTime\InPos,0;
        WaitSyncTask sync49,task_list;
        WaitSyncTask\InPos,sync50,task_list;
        AccSet 100,100,\FinePointRamp:=100;
        MoveL rotate_BFR,v800,z5,Servo\WObj:=wobj0;
        close_gripper_L;
        p_rotate_180_sub_L;
        ! Xoay 2 lan
        WaitTime\InPos,0;
        open_gripper_L;
        AccSet 100,100,\FinePointRamp:=100;
        MoveL TG_rotate_x2,v800,z5,Servo\WObj:=wobj0;
        !Chen If...Else
        IF (next="B") OR (next="B'") OR (next="B2") THEN
            p_TG_case_BF_FB_hL;
        ELSEIF (next="R") OR (next="R'") OR (next="R2") THEN
            p_TG_case_BR_FR_hL;
        ELSE
            MoveL waitpoint,v800,z5,Servo\WObj:=wobj0;
            WaitSyncTask sync51,task_list;
            WaitSyncTask\InPos,sync52,task_list;
            AccSet 100,100,\FinePointRamp:=100;
            MoveL center,v800,z5,Servo\WObj:=wobj0;
            close_gripper_L;
            WaitSyncTask sync53,task_list;
        ENDIF
        sol:="";
        fn:="FNF2";
        next:="";
    ENDPROC

    PROC p_rotate_L_CCW()
        MotionSup \On \TuneValue:=210;
        !Chen If...Else
        IF (next="U") OR (next="U'") OR (next="U2") OR (next="D") OR (next="D'") OR (next="D2") THEN
            p_TG_case_LD_LU_hL;
            !        ELSEIF (next="D") OR (next="D'") OR (next="D2") THEN
            !            p_TG_case_LD_LU_hL;
        ELSE
            WaitSyncTask sync54,task_list;
        ENDIF
        sol:="";
        fn:="FNLN";
        next:="";
    ENDPROC

    PROC p_rotate_L_CW()
        MotionSup \On \TuneValue:=210;
        !Chen If...Else
        IF (next="U") OR (next="U'") OR (next="U2") OR (next="D") OR (next="D'") OR (next="D2") THEN
            p_TG_case_LD_LU_hL;
            !        ELSEIF (next="D") OR (next="D'") OR (next="D2") THEN
            !            p_TG_case_LD_LU_hL;
        ELSE
            WaitSyncTask sync55,task_list;
        ENDIF
        sol:="";
        fn:="FNL";
        next:="";
    ENDPROC

    PROC p_rotate_L_x2()
        MotionSup \On \TuneValue:=210;
        !Chen If...Else
        IF (next="U") OR (next="U'") OR (next="U2") OR (next="D") OR (next="D'") OR (next="D2") THEN
            p_TG_case_LD_LU_hL;
            !        ELSEIF (next="D") OR (next="D'") OR (next="D2") THEN
            !            p_TG_case_LD_LU_hL;
        ELSE
            WaitSyncTask sync56,task_list;
        ENDIF
        sol:="";
        fn:="FNL2";
        next:="";
    ENDPROC

    PROC p_lib_rotate_L()

        IF isConnected THEN
            IF number="1" THEN
                p_case_1_L;
                number:="";
            ELSEIF number="2" THEN
                p_case_2_L;
                number:="";
            ELSEIF number="3" THEN
                p_case_3_L;
                number:="";
            ELSEIF number="4" THEN
                p_case_4_L;
                number:="";
            ELSEIF number="5" THEN
                p_case_5_L;
                number:="";
            ELSEIF number="6" THEN
                p_case_6_L;
                number:="";
            ELSEIF number="7" THEN
                p_case_7_L;
                number:="";
            ELSEIF number="8" THEN
                p_case_8_L;
                number:="";
            ELSEIF number="9" THEN
                p_case_9_L;
                number:="";
            ELSEIF number="10" THEN
                p_case_10_L;
                number:="";
            ELSEIF number="11" THEN
                p_case_11_L;
                number:="";
            ELSEIF number="12" THEN
                p_case_12_L;
                number:="";
            ELSEIF number="13" THEN
                p_case_13_L;
                number:="";
            ELSEIF number="14" THEN
                p_case_14_L;
                number:="";
            ELSEIF number="15" THEN
                p_case_15_L;
                number:="";
            ELSEIF number="16" THEN
                p_case_16_L;
                number:="";
            ELSEIF number="17" THEN
                p_case_17_L;
                number:="";
            ELSEIF number="18" THEN
                p_case_18_L;
                number:="";
            ELSEIF number="19" THEN
                p_case_19_L;
                number:="";
            ELSEIF number="20" THEN
                p_case_20_L;
                number:="";
            ELSEIF number="21" THEN
                p_case_21_L;
                number:="";
            ELSEIF number="22" THEN
                p_case_22_L;
                number:="";
            ELSEIF number="23" THEN
                p_case_23_L;
                number:="";
            ELSEIF number="24" THEN
                p_case_24_L;
                number:="";

            ELSEIF sol="L" THEN
                p_rotate_L_CW;
            ELSEIF sol="L'" THEN
                p_rotate_L_CCW;
            ELSEIF sol="R" THEN
                p_rotate_R_CW;
            ELSEIF sol="R'" THEN
                p_rotate_R_CCW;
            ELSEIF sol="B" THEN
                p_rotate_B_CW;
            ELSEIF sol="B'" THEN
                p_rotate_B_CCW;
            ELSEIF sol="F" THEN
                p_rotate_F_CW;
            ELSEIF sol="F'" THEN
                p_rotate_F_CCW;
            ELSEIF sol="U" THEN
                p_rotate_U_CW;
            ELSEIF sol="U'" THEN
                p_rotate_U_CCW;
            ELSEIF sol="D" THEN
                p_rotate_D_CW;
            ELSEIF sol="D'" THEN
                p_rotate_D_CCW;
            ELSEIF sol="L2" THEN
                p_rotate_L_x2;
            ELSEIF sol="R2" THEN
                p_rotate_R_x2;
            ELSEIF sol="B2" THEN
                p_rotate_B_x2;
            ELSEIF sol="F2" THEN
                p_rotate_F_x2;
            ELSEIF sol="U2" THEN
                p_rotate_U_x2;
            ELSEIF sol="D2" THEN
                p_rotate_D_x2;
            ELSEIF sol="E" THEN
                p_end_L;
                sol:="";
                fn:="FNE";
            ENDIF
        ENDIF
    ENDPROC

    PROC p_TG_case_BF_FB_hL()
        WaitSyncTask syncBF_FB,task_list;
    ENDPROC

    PROC p_TG_case_BR_FR_hL()
        WaitSyncTask\InPos, syncBF_R1,task_list;
        AccSet 100,100,\FinePointRamp:=100;
        MoveJ waitpoint,v800,z100,Servo\WObj:=wobj0;
        WaitSyncTask syncBF_R2,task_list;
    ENDPROC

    PROC p_TG_case_RF_RB_hL()
        WaitSyncTask\InPos, syncR_BF,task_list;
    ENDPROC

    PROC p_TG_case_UD_DU_hL()
        WaitSyncTask syncDU_UD,task_list;
    ENDPROC

    PROC p_TG_case_UL_DL_hL()
        WaitSyncTask\InPos, syncUD_L1,task_list;
        AccSet 100,100,\FinePointRamp:=100;
        MoveJ waitpoint,v800,z100,Servo\WObj:=wobj0;
        MoveL center,v800,z5,Servo\WObj:=wobj0;
        WaitSyncTask syncUD_L2,task_list;
    ENDPROC

    PROC p_TG_case_LD_LU_hL()
        WaitSyncTask\InPos, syncL_DU,task_list;
    ENDPROC
ENDMODULE