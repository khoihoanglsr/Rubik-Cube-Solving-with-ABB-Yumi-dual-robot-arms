MODULE Rotate_Total_L
    PROC p_rotate_180_add_L()
        handL_rotate:=CJointT();
        handL_rotate.robax.rax_6:=handL_rotate.robax.rax_6+180;
        AccSet 100,100,\FinePointRamp:=100;
        MoveAbsJ handL_rotate,v800,z5,tGripper;
    ENDPROC

    PROC p_rotate_180_sub_L()
        handL_rotate:=CJointT();
        handL_rotate.robax.rax_6:=handL_rotate.robax.rax_6-180;
        AccSet 100,100,\FinePointRamp:=100;
        MoveAbsJ handL_rotate,v800,z5,tGripper;
    ENDPROC

    PROC p_rotate_90_add_L()
        handL_rotate:=CJointT();
        handL_rotate.robax.rax_6:=handL_rotate.robax.rax_6+90;
        AccSet 100,100,\FinePointRamp:=100;
        MoveAbsJ handL_rotate,v800,z5,tGripper;
    ENDPROC

    PROC p_rotate_90_sub_L()
        handL_rotate:=CJointT();
        handL_rotate.robax.rax_6:=handL_rotate.robax.rax_6-90;
        AccSet 100,100,\FinePointRamp:=100;
        MoveAbsJ handL_rotate,v800,z5,tGripper;
    ENDPROC

    PROC p_rotate_155_add_L()
        handL_rotate:=CJointT();
        handL_rotate.robax.rax_6:=handL_rotate.robax.rax_6+155;
        AccSet 100,100,\FinePointRamp:=100;
        MoveAbsJ handL_rotate,v800,z5,tGripper;
    ENDPROC

    PROC p_rotate_155_sub_L()
        handL_rotate:=CJointT();
        handL_rotate.robax.rax_6:=handL_rotate.robax.rax_6-155;
        AccSet 100,100,\FinePointRamp:=100;
        MoveAbsJ handL_rotate,v800,z5,tGripper;
    ENDPROC

    PROC p_rotate_25_add_L()
        handL_rotate:=CJointT();
        handL_rotate.robax.rax_6:=handL_rotate.robax.rax_6+25;
        AccSet 100,100,\FinePointRamp:=100;
        MoveAbsJ handL_rotate,v800,z5,tGripper;
    ENDPROC

    PROC p_rotate_25_sub_L()
        handL_rotate:=CJointT();
        handL_rotate.robax.rax_6:=handL_rotate.robax.rax_6-25;
        AccSet 100,100,\FinePointRamp:=100;
        MoveAbsJ handL_rotate,v800,z5,tGripper;
    ENDPROC
ENDMODULE