MODULE Check_Face_BFR_R

    PROC p_pick()
        AccSet 100,100,\FinePointRamp:=100;
        MoveJ TG_pick,v800,z100,Servo\WObj:=wobj0;
        MoveJ pick_up,v800,z100,Servo\WObj:=wobj0;
        WaitTime\InPos,0.5;
        open_gripper_R;
        WaitTime\InPos,1.5;
        AccSet 100,100,\FinePointRamp:=100;
        MoveL pick,v800,z5,Servo\WObj:=wobj0;
        close_gripper_R;
        AccSet 100,100,\FinePointRamp:=100;
        MoveL pick_up,v800,z5,Servo\WObj:=wobj0;
    ENDPROC

    PROC p_check_BFR_R()
        p_pick;
        AccSet 100,100,\FinePointRamp:=100;
        MoveJ check_BFR,v800,z100,Servo\WObj:=wobj0;
        ! check_B
        WaitTime\InPos,0.5;
        WaitSyncTask\InPos,sync75,task_list;
        WaitTime\InPos,2;
        p_rotate_180_add_R;
        ! check_F
        WaitTime\InPos,0.5;
        WaitSyncTask\InPos,sync76,task_list;
        WaitTime\InPos,2;
        p_rotate_180_sub_R;
        ! ve check_BFR
        WaitTime 2;
        AccSet 100,100,\FinePointRamp:=100;
        MoveJ check_R,v800,z100,Servo\WObj:=wobj0;
        ! check_R
        WaitTime\InPos,0.5;
        WaitSyncTask\InPos,sync77,task_list;
        WaitTime\InPos,2;
        AccSet 100,100,\FinePointRamp:=100;
        MoveJ check_BFR,v800,z100,Servo\WObj:=wobj0;
        WaitTime 2;
        WaitSyncTask\InPos,sync2,task_list;
        WaitSyncTask\InPos,sync3,task_list;
        WaitTime\InPos,0;
        open_gripper_R;
        AccSet 100,100,\FinePointRamp:=100;
        MoveL TG_check_BFR,v800,z5,Servo\WObj:=wobj0;
        MoveL waitpoint,v800,z5,Servo\WObj:=wobj0;
        WaitSyncTask sync4,task_list;
        WaitSyncTask\InPos,sync5,task_list;
    ENDPROC
ENDMODULE