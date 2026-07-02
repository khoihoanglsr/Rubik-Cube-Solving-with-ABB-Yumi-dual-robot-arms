MODULE Check_Face_DUL_L

    VAR string cap:="";

    PROC p_check_DUL_L()
        WaitSyncTask\InPos,sync75,task_list;
        WaitTime 1;
        cap:="CAP1";
        WaitSyncTask\InPos,sync76,task_list;
        WaitTime 1;
        cap:="CAP2";
        WaitSyncTask\InPos,sync77,task_list;
        WaitTime 1;
        cap:="CAP3";
        WaitSyncTask\InPos,sync2,task_list;
        AccSet 100,100,\FinePointRamp:=100;
        MoveJ TG_check_DUL,v800,z100,Servo\WObj:=wobj0;
        WaitTime\InPos,0.5;
        open_gripper_L;
        WaitTime\InPos,1.5;
        AccSet 100,100,\FinePointRamp:=100;
        MoveL check_DUL,v800,z5,Servo\WObj:=wobj0;
        close_gripper_L;
        WaitSyncTask\InPos,sync3,task_list;
        WaitSyncTask sync4,task_list;
        WaitTime\InPos,1;
        p_rotate_90_add_L;
        ! check_D
        WaitTime\InPos,0.5;
        WaitTime 1;
        cap:="CAP4";
        WaitTime\InPos,2;
        p_rotate_90_sub_L;
        ! Ve check_DUL
        WaitTime 2;
        p_rotate_90_sub_L;
        ! check U
        WaitTime\InPos,0.5;
        WaitTime 1;
        cap:="CAP5";
        WaitTime\InPos,2;
        !        p_rotate_90_add_L;
        !        ! Ve check_DUL
        !        WaitTime 2;
        AccSet 100,100,\FinePointRamp:=100;
        MoveJ check_L,v800,z100,Servo\WObj:=wobj0;
        ! check_L
        WaitTime\InPos,0.5;
        WaitTime 1;
        cap:="CAP6";
        WaitTime\InPos,2;
        AccSet 100,100,\FinePointRamp:=100;
        MoveJ check_DUL,v800,z100,Servo\WObj:=wobj0;
        MoveL center,v800,z5,Servo\WObj:=wobj0;
        WaitSyncTask\InPos,sync5,task_list;
    ENDPROC
ENDMODULE