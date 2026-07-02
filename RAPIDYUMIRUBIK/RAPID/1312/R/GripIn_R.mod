MODULE GripIn_R
    PROC close_gripper_R()
        WaitTime\InPos,0;
        g_GripIn \holdForce:= 14;
!        WaitTime\InPos,0;
    ENDPROC
ENDMODULE