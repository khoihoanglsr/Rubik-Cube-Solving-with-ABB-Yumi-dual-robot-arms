MODULE Lib_Rotate_HandR

    VAR string sol:="";
    VAR string number:="";
    VAR string fn:="";
    VAR string next:="";

    PROC p_end_R()
        WaitSyncTask\InPos,sync7,task_list;
        MotionSup \On \TuneValue:=210;
        AccSet 100,100,\FinePointRamp:=100;
        MoveL waitpoint,v800,z5,Servo\WObj:=wobj0;
        MoveJ pick_up,v800,z100,Servo\WObj:=wobj0;
        MoveL pick,v800,z5,Servo\WObj:=wobj0;
        WaitTime\InPos,0.1;
        open_gripper_R;
        AccSet 100,100,\FinePointRamp:=100;
        MoveL pick_up,v800,z5,Servo\WObj:=wobj0;
        MoveJ TG_pick,v800,z100,Servo\WObj:=wobj0;
        p_home_R;
        WaitSyncTask\InPos,sync8,task_list;
    ENDPROC

    PROC p_rotate_B_CCW()
        WaitSyncTask sync9,task_list;
        MotionSup \On \TuneValue:=210;
        AccSet 100,100,\FinePointRamp:=100;
        MoveJ rotate_B,v800,z100,Servo\WObj:=wobj0;
        WaitSyncTask\InPos,sync10,task_list;
        !Chen If...Else
        IF (next="F") OR (next="F'") OR (next="F2") THEN
            p_TG_case_BF_FB_hR;
        ELSEIF (next="R") OR (next="R'") OR (next="R2") THEN
            p_TG_case_BR_FR_hR;
        ELSE
            WaitSyncTask sync11,task_list;
            AccSet 100,100,\FinePointRamp:=100;
            MoveJ center,v800,z100,Servo\WObj:=wobj0;
            WaitSyncTask\InPos,sync12,task_list;
            WaitSyncTask sync13,task_list;
        ENDIF
        sol:="";
        fn:="FNBN";
        next:="";
    ENDPROC

    PROC p_rotate_B_CW()
        WaitSyncTask sync14,task_list;
        MotionSup \On \TuneValue:=210;
        AccSet 100,100,\FinePointRamp:=100;
        MoveJ rotate_B,v800,z100,Servo\WObj:=wobj0;
        WaitSyncTask\InPos,sync15,task_list;
        !Chen If...Else
        IF (next="F") OR (next="F'") OR (next="F2") THEN
            p_TG_case_BF_FB_hR;
        ELSEIF (next="R") OR (next="R'") OR (next="R2") THEN
            p_TG_case_BR_FR_hR;
        ELSE
            WaitSyncTask sync16,task_list;
            AccSet 100,100,\FinePointRamp:=100;
            MoveJ center,v800,z100,Servo\WObj:=wobj0;
            WaitSyncTask\InPos,sync17,task_list;
            WaitSyncTask sync18,task_list;
        ENDIF
        sol:="";
        fn:="FNB";
        next:="";
    ENDPROC

    PROC p_rotate_B_x2()
        WaitSyncTask\InPos,sync19,task_list;
        MotionSup \On \TuneValue:=210;
        AccSet 100,100,\FinePointRamp:=100;
        MoveJ rotate_B,v800,z100,Servo\WObj:=wobj0;
        WaitSyncTask\InPos,sync20,task_list;
        !Chen If...Else
        IF (next="F") OR (next="F'") OR (next="F2") THEN
            p_TG_case_BF_FB_hR;
        ELSEIF (next="R") OR (next="R'") OR (next="R2") THEN
            p_TG_case_BR_FR_hR;
        ELSE
            WaitSyncTask sync21,task_list;
            AccSet 100,100,\FinePointRamp:=100;
            MoveJ center,v800,z100,Servo\WObj:=wobj0;
            WaitSyncTask\InPos,sync22,task_list;
            WaitSyncTask sync23,task_list;
        ENDIF
        sol:="";
        fn:="FNB2";
        next:="";
    ENDPROC

    PROC p_rotate_D_CCW()
        MotionSup \On \TuneValue:=210;
        open_gripper_R;
        AccSet 100,100,\FinePointRamp:=100;
        MoveL waitpoint,v800,z5,Servo\WObj:=wobj0;
        WaitTime\InPos,0;
        WaitSyncTask sync24,task_list;
        WaitSyncTask\InPos,sync25,task_list;
        AccSet 100,100,\FinePointRamp:=100;
        MoveL rotate_DUL,v800,z5,Servo\WObj:=wobj0;
        close_gripper_R;
        p_rotate_90_sub_R;
        !Xoay nghich
        WaitTime\InPos,0.1;
        open_gripper_R;
        AccSet 100,100,\FinePointRamp:=100;
        MoveL TG_rotate_CCW,v800,z5,Servo\WObj:=wobj0;
        !Chen If...Else
        IF (next="U") OR (next="U'") OR (next="U2") THEN
            p_TG_case_UD_DU_hR;
        ELSEIF (next="L") OR (next="L'") OR (next="L2") THEN
            p_TG_case_UL_DL_hR;
        ELSE
            MoveL waitpoint,v800,z5,Servo\WObj:=wobj0;
            WaitSyncTask sync26,task_list;
            WaitSyncTask\InPos,sync27,task_list;
            AccSet 100,100,\FinePointRamp:=100;
            MoveL center,v800,z5,Servo\WObj:=wobj0;
            close_gripper_R;
            WaitSyncTask sync28,task_list;
        ENDIF
        sol:="";
        fn:="FNDN";
        next:="";
    ENDPROC

    PROC p_rotate_D_CW()
        MotionSup \On \TuneValue:=210;
        open_gripper_R;
        AccSet 100,100,\FinePointRamp:=100;
        MoveL waitpoint,v800,z5,Servo\WObj:=wobj0;
        WaitTime\InPos,0;
        WaitSyncTask sync29,task_list;
        WaitSyncTask\InPos,sync30,task_list;
        AccSet 100,100,\FinePointRamp:=100;
        MoveL rotate_DUL,v800,z5,Servo\WObj:=wobj0;
        close_gripper_R;
        p_rotate_90_add_R;
        !Xoay thuan
        WaitTime\InPos,0.1;
        open_gripper_R;
        AccSet 100,100,\FinePointRamp:=100;
        MoveL TG_rotate_CW,v800,z5,Servo\WObj:=wobj0;
        !Chen If...Else
        IF (next="U") OR (next="U'") OR (next="U2") THEN
            p_TG_case_UD_DU_hR;
        ELSEIF (next="L") OR (next="L'") OR (next="L2") THEN
            p_TG_case_UL_DL_hR;
        ELSE
            MoveL waitpoint,v800,z5,Servo\WObj:=wobj0;
            WaitSyncTask sync31,task_list;
            WaitSyncTask\InPos,sync32,task_list;
            AccSet 100,100,\FinePointRamp:=100;
            MoveL center,v800,z5,Servo\WObj:=wobj0;
            close_gripper_R;
            WaitSyncTask sync33,task_list;
        ENDIF
        sol:="";
        fn:="FND";
        next:="";
    ENDPROC

    PROC p_rotate_D_x2()
        MotionSup \On \TuneValue:=210;
        open_gripper_R;
        AccSet 100,100,\FinePointRamp:=100;
        MoveL waitpoint,v800,z5,Servo\WObj:=wobj0;
        WaitTime\InPos,0;
        WaitSyncTask sync34,task_list;
        WaitSyncTask\InPos,sync35,task_list;
        AccSet 100,100,\FinePointRamp:=100;
        MoveL rotate_DUL,v800,z5,Servo\WObj:=wobj0;
        close_gripper_R;
        p_rotate_180_add_R;
        !Xoay 2 lan
        WaitTime\InPos,0.1;
        open_gripper_R;
        AccSet 100,100,\FinePointRamp:=100;
        MoveL TG_rotate_x2,v800,z5,Servo\WObj:=wobj0;
        !Chen If...Else
        IF (next="U") OR (next="U'") OR (next="U2") THEN
            p_TG_case_UD_DU_hR;
        ELSEIF (next="L") OR (next="L'") OR (next="L2") THEN
            p_TG_case_UL_DL_hR;
        ELSE
            MoveL waitpoint,v800,z5,Servo\WObj:=wobj0;
            WaitSyncTask sync36,task_list;
            WaitSyncTask\InPos,sync37,task_list;
            AccSet 100,100,\FinePointRamp:=100;
            MoveL center,v800,z5,Servo\WObj:=wobj0;
            close_gripper_R;
            WaitSyncTask sync38,task_list;
        ENDIF
        sol:="";
        fn:="FND2";
        next:="";
    ENDPROC

    PROC p_rotate_F_CCW()
        WaitSyncTask sync39,task_list;
        MotionSup \On \TuneValue:=210;
        AccSet 100,100,\FinePointRamp:=100;
        MoveJ rotate_F,v800,z100,Servo\WObj:=wobj0;
        WaitSyncTask\InPos,sync40,task_list;
        !Chen If...Else
        IF (next="B") OR (next="B'") OR (next="B2") THEN
            p_TG_case_BF_FB_hR;
        ELSEIF (next="R") OR (next="R'") OR (next="R2") THEN
            p_TG_case_BR_FR_hR;
        ELSE
            WaitSyncTask sync41,task_list;
            AccSet 100,100,\FinePointRamp:=100;
            MoveJ center,v800,z100,Servo\WObj:=wobj0;
            WaitSyncTask\InPos,sync42,task_list;
            WaitSyncTask sync43,task_list;
        ENDIF
        sol:="";
        fn:="FNFN";
        next:="";
    ENDPROC

    PROC p_rotate_F_CW()
        WaitSyncTask sync44,task_list;
        MotionSup \On \TuneValue:=210;
        AccSet 100,100,\FinePointRamp:=100;
        MoveJ rotate_F,v800,z100,Servo\WObj:=wobj0;
        WaitSyncTask\InPos,sync45,task_list;
        !Chen If...Else
        IF (next="B") OR (next="B'") OR (next="B2") THEN
            p_TG_case_BF_FB_hR;
        ELSEIF (next="R") OR (next="R'") OR (next="R2") THEN
            p_TG_case_BR_FR_hR;
        ELSE
            WaitSyncTask sync46,task_list;
            AccSet 100,100,\FinePointRamp:=100;
            MoveJ center,v800,z100,Servo\WObj:=wobj0;
            WaitSyncTask\InPos,sync47,task_list;
            WaitSyncTask sync48,task_list;
        ENDIF
        sol:="";
        fn:="FNF";
        next:="";
    ENDPROC

    PROC p_rotate_F_x2()
        WaitSyncTask sync49,task_list;
        MotionSup \On \TuneValue:=210;
        AccSet 100,100,\FinePointRamp:=100;
        MoveJ rotate_F,v800,z100,Servo\WObj:=wobj0;
        WaitSyncTask\InPos,sync50,task_list;
        !Chen If...Else
        IF (next="B") OR (next="B'") OR (next="B2") THEN
            p_TG_case_BF_FB_hR;
        ELSEIF (next="R") OR (next="R'") OR (next="R2") THEN
            p_TG_case_BR_FR_hR;
        ELSE
            WaitSyncTask sync51,task_list;
            AccSet 100,100,\FinePointRamp:=100;
            MoveJ center,v800,z100,Servo\WObj:=wobj0;
            WaitSyncTask\InPos,sync52,task_list;
            WaitSyncTask sync53,task_list;
        ENDIF
        sol:="";
        fn:="FNF2";
        next:="";
    ENDPROC

    PROC p_rotate_L_CW()
        MotionSup \On \TuneValue:=210;
        open_gripper_R;
        AccSet 100,100,\FinePointRamp:=100;
        MoveL rotate_DUL,v800,z5,Servo\WObj:=wobj0;
        close_gripper_R;
        p_rotate_90_add_R;
        ! Xoay thuan
        WaitTime\InPos,0.1;
        open_gripper_R;
        AccSet 100,100,\FinePointRamp:=100;
        MoveL TG_rotate_CW,v800,z5,Servo\WObj:=wobj0;
        !Chen If...Else
        IF (next="U") OR (next="U'") OR (next="U2") OR (next="D") OR (next="D'") OR (next="D2") THEN
            p_TG_case_LU_LD_hR;
            !        ELSEIF (next="D") OR (next="D'") OR (next="D2") THEN
            !            p_TG_case_LU_LD_hR;
        ELSE
            MoveL waitpoint,v800,z5,Servo\WObj:=wobj0;
            MoveL center,v800,z5,Servo\WObj:=wobj0;
            close_gripper_R;
            WaitSyncTask sync55,task_list;
        ENDIF
        sol:="";
        fn:="FNL";
        next:="";
    ENDPROC

    PROC p_rotate_L_CCW()
        MotionSup \On \TuneValue:=210;
        open_gripper_R;
        AccSet 100,100,\FinePointRamp:=100;
        MoveL rotate_DUL,v800,z5,Servo\WObj:=wobj0;
        close_gripper_R;
        p_rotate_90_sub_R;
        ! Xoay nghich
        WaitTime\InPos,0.1;
        open_gripper_R;
        AccSet 100,100,\FinePointRamp:=100;
        MoveL TG_rotate_CCW,v800,z5,Servo\WObj:=wobj0;
        !Chen If...Else
        IF (next="U") OR (next="U'") OR (next="U2") OR (next="D") OR (next="D'") OR (next="D2") THEN
            p_TG_case_LU_LD_hR;
            !        ELSEIF (next="D") OR (next="D'") OR (next="D2") THEN
            !            p_TG_case_LU_LD_hR;
        ELSE
            MoveL waitpoint,v800,z5,Servo\WObj:=wobj0;
            MoveL center,v800,z5,Servo\WObj:=wobj0;
            close_gripper_R;
            WaitSyncTask sync54,task_list;
        ENDIF
        sol:="";
        fn:="FNLN";
        next:="";
    ENDPROC

    PROC p_rotate_L_x2()
        MotionSup \On \TuneValue:=210;
        open_gripper_R;
        AccSet 100,100,\FinePointRamp:=100;
        MoveL rotate_DUL,v800,z5,Servo\WObj:=wobj0;
        close_gripper_R;
        p_rotate_180_add_R;
        !Xoay 2 lan
        WaitTime\InPos,0.1;
        open_gripper_R;
        AccSet 100,100,\FinePointRamp:=100;
        MoveL TG_rotate_x2,v800,z5,Servo\WObj:=wobj0;
        !Chen If...Else
        IF (next="U") OR (next="U'") OR (next="U2") OR (next="D") OR (next="D'") OR (next="D2") THEN
            p_TG_case_LU_LD_hR;
            !        ELSEIF (next="D") OR (next="D'") OR (next="D2") THEN
            !            p_TG_case_LU_LD_hR;
        ELSE
            MoveL waitpoint,v800,z5,Servo\WObj:=wobj0;
            MoveL center,v800,z5,Servo\WObj:=wobj0;
            close_gripper_R;
            WaitSyncTask sync56,task_list;
        ENDIF
        sol:="";
        fn:="FNL2";
        next:="";
    ENDPROC

    PROC p_rotate_R_CCW()
        MotionSup \On \TuneValue:=210;
        !Chen If...Else
        IF (next="F") OR (next="F'") OR (next="F2") OR (next="B") OR (next="B'") OR (next="B2") THEN
            p_TG_case_RF_RB_hR;
            !        ELSEIF (next="B") OR (next="B'") OR (next="B2") THEN
            !            p_TG_case_RF_RB_hR;
        ELSE
            WaitSyncTask sync57,task_list;
        ENDIF
        sol:="";
        fn:="FNRN";
        next:="";
    ENDPROC

    PROC p_rotate_R_CW()
        MotionSup \On \TuneValue:=210;
        !Chen If...Else
        IF (next="F") OR (next="F'") OR (next="F2") OR (next="B") OR (next="B'") OR (next="B2") THEN
            p_TG_case_RF_RB_hR;
            !        ELSEIF (next="B") OR (next="B'") OR (next="B2") THEN
            !            p_TG_case_RF_RB_hR;
        ELSE
            WaitSyncTask sync58,task_list;
        ENDIF
        sol:="";
        fn:="FNR";
        next:="";
    ENDPROC

    PROC p_rotate_R_x2()
        MotionSup \On \TuneValue:=210;
        !Chen If...Else
        IF (next="F") OR (next="F'") OR (next="F2") OR (next="B") OR (next="B'") OR (next="B2") THEN
            p_TG_case_RF_RB_hR;
            !        ELSEIF (next="B") OR (next="B'") OR (next="B2") THEN
            !            p_TG_case_RF_RB_hR;
        ELSE
            WaitSyncTask sync59,task_list;
        ENDIF
        sol:="";
        fn:="FNR2";
        next:="";
    ENDPROC

    PROC p_rotate_U_CCW()
        MotionSup \On \TuneValue:=210;
        open_gripper_R;
        AccSet 100,100,\FinePointRamp:=100;
        MoveL waitpoint,v800,z5,Servo\WObj:=wobj0;
        WaitTime\InPos,0;
        WaitSyncTask sync60,task_list;
        WaitSyncTask\InPos,sync61,task_list;
        AccSet 100,100,\FinePointRamp:=100;
        MoveL rotate_DUL,v800,z5,Servo\WObj:=wobj0;
        close_gripper_R;
        p_rotate_90_sub_R;
        ! Xoay nghich
        WaitTime\InPos,0.1;
        open_gripper_R;
        AccSet 100,100,\FinePointRamp:=100;
        MoveL TG_rotate_CCW,v800,z5,Servo\WObj:=wobj0;
        !Chen If...Else
        IF (next="D") OR (next="D'") OR (next="D2") THEN
            p_TG_case_UD_DU_hR;
        ELSEIF (next="L") OR (next="L'") OR (next="L2") THEN
            p_TG_case_UL_DL_hR;
        ELSE
            MoveL waitpoint,v800,z5,Servo\WObj:=wobj0;
            WaitSyncTask sync62,task_list;
            WaitSyncTask\InPos,sync63,task_list;
            AccSet 100,100,\FinePointRamp:=100;
            MoveL center,v800,z5,Servo\WObj:=wobj0;
            close_gripper_R;
            WaitSyncTask sync64,task_list;
        ENDIF
        sol:="";
        fn:="FNUN";
        next:="";
    ENDPROC

    PROC p_rotate_U_CW()
        MotionSup \On \TuneValue:=210;
        open_gripper_R;
        AccSet 100,100,\FinePointRamp:=100;
        MoveL waitpoint,v800,z5,Servo\WObj:=wobj0;
        WaitTime\InPos,0;
        WaitSyncTask sync65,task_list;
        WaitSyncTask\InPos,sync66,task_list;
        AccSet 100,100,\FinePointRamp:=100;
        MoveL rotate_DUL,v800,z5,Servo\WObj:=wobj0;
        close_gripper_R;
        p_rotate_90_add_R;
        ! Xoay thuan
        WaitTime\InPos,0.1;
        open_gripper_R;
        AccSet 100,100,\FinePointRamp:=100;
        MoveL TG_rotate_CW,v800,z5,Servo\WObj:=wobj0;
        !Chen If...Else
        IF (next="D") OR (next="D'") OR (next="D2") THEN
            p_TG_case_UD_DU_hR;
        ELSEIF (next="L") OR (next="L'") OR (next="L2") THEN
            p_TG_case_UL_DL_hR;
        ELSE
            MoveL waitpoint,v800,z5,Servo\WObj:=wobj0;
            WaitSyncTask sync67,task_list;
            WaitSyncTask\InPos,sync68,task_list;
            AccSet 100,100,\FinePointRamp:=100;
            MoveL center,v800,z5,Servo\WObj:=wobj0;
            close_gripper_R;
            WaitSyncTask sync69,task_list;
        ENDIF
        sol:="";
        fn:="FNU";
        next:="";
    ENDPROC

    PROC p_rotate_U_x2()
        MotionSup \On \TuneValue:=210;
        open_gripper_R;
        AccSet 100,100,\FinePointRamp:=100;
        MoveL waitpoint,v800,z5,Servo\WObj:=wobj0;
        WaitTime\InPos,0;
        WaitSyncTask sync70,task_list;
        WaitSyncTask\InPos,sync71,task_list;
        AccSet 100,100,\FinePointRamp:=100;
        MoveL rotate_DUL,v800,z5,Servo\WObj:=wobj0;
        close_gripper_R;
        p_rotate_180_add_R;
        ! Xoay 2 lan
        WaitTime\InPos,0.1;
        open_gripper_R;
        AccSet 100,100,\FinePointRamp:=100;
        MoveL TG_rotate_x2,v800,z5,Servo\WObj:=wobj0;
        !Chen If...Else
        IF (next="D") OR (next="D'") OR (next="D2") THEN
            p_TG_case_UD_DU_hR;
        ELSEIF (next="L") OR (next="L'") OR (next="L2") THEN
            p_TG_case_UL_DL_hR;
        ELSE
            MoveL waitpoint,v800,z5,Servo\WObj:=wobj0;
            WaitSyncTask sync72,task_list;
            WaitSyncTask\InPos,sync73,task_list;
            AccSet 100,100,\FinePointRamp:=100;
            MoveL center,v800,z5,Servo\WObj:=wobj0;
            close_gripper_R;
            WaitSyncTask sync74,task_list;
        ENDIF
        sol:="";
        fn:="FNU2";
        next:="";
    ENDPROC

    PROC p_lib_rotate_R()

        IF isConnected THEN
            IF number="1" THEN
                p_case_1_R;
                number:="";
            ELSEIF number="2" THEN
                p_case_2_R;
                number:="";
            ELSEIF number="3" THEN
                p_case_3_R;
                number:="";
            ELSEIF number="4" THEN
                p_case_4_R;
                number:="";
            ELSEIF number="5" THEN
                p_case_5_R;
                number:="";
            ELSEIF number="6" THEN
                p_case_6_R;
                number:="";
            ELSEIF number="7" THEN
                p_case_7_R;
                number:="";
            ELSEIF number="8" THEN
                p_case_8_R;
                number:="";
            ELSEIF number="9" THEN
                p_case_9_R;
                number:="";
            ELSEIF number="10" THEN
                p_case_10_R;
                number:="";
            ELSEIF number="11" THEN
                p_case_11_R;
                number:="";
            ELSEIF number="12" THEN
                p_case_12_R;
                number:="";
            ELSEIF number="13" THEN
                p_case_13_R;
                number:="";
            ELSEIF number="14" THEN
                p_case_14_R;
                number:="";
            ELSEIF number="15" THEN
                p_case_15_R;
                number:="";
            ELSEIF number="16" THEN
                p_case_16_R;
                number:="";
            ELSEIF number="17" THEN
                p_case_17_R;
                number:="";
            ELSEIF number="18" THEN
                p_case_18_R;
                number:="";
            ELSEIF number="19" THEN
                p_case_19_R;
                number:="";
            ELSEIF number="20" THEN
                p_case_20_R;
                number:="";
            ELSEIF number="21" THEN
                p_case_21_R;
                number:="";
            ELSEIF number="22" THEN
                p_case_22_R;
                number:="";
            ELSEIF number="23" THEN
                p_case_23_R;
                number:="";
            ELSEIF number="24" THEN
                p_case_24_R;
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
                p_end_R;
                sol:="";
                fn:="FNE";
            ENDIF
        ENDIF
    ENDPROC

    PROC p_TG_case_BF_FB_hR()
        WaitSyncTask syncBF_FB,task_list;
    ENDPROC

    PROC p_TG_case_BR_FR_hR()
        WaitSyncTask\InPos, syncBF_R1,task_list;
        AccSet 100,100,\FinePointRamp:=100;
        MoveJ waitpoint,v800,z100,Servo\WObj:=wobj0;
        MoveL center,v800,z5,Servo\WObj:=wobj0;
        WaitSyncTask syncBF_R2,task_list;
    ENDPROC

    PROC p_TG_case_RF_RB_hR()
        WaitSyncTask\InPos, syncR_BF,task_list;
    ENDPROC

    PROC p_TG_case_UD_DU_hR()
        WaitSyncTask syncDU_UD,task_list;
    ENDPROC

    PROC p_TG_case_UL_DL_hR()
        WaitSyncTask\InPos, syncUD_L1,task_list;
        AccSet 100,100,\FinePointRamp:=100;
        MoveJ waitpoint,v800,z100,Servo\WObj:=wobj0;
        WaitSyncTask syncUD_L2,task_list;
    ENDPROC

    PROC p_TG_case_LU_LD_hR()
        WaitSyncTask\InPos,syncL_DU,task_list;
    ENDPROC
ENDMODULE