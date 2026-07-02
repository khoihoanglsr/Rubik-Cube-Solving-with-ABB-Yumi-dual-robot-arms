MODULE Rotate_Total_R
    PROC p_rotate_180_add_R()
        handR_rotate:=CJointT();
        handR_rotate.robax.rax_6:=handR_rotate.robax.rax_6+180;
        AccSet 100,100,\FinePointRamp:=100;
        MoveAbsJ handR_rotate,v800,z5,tGripper;
    ENDPROC

    PROC p_rotate_90_add_R()
        handR_rotate:=CJointT();
        handR_rotate.robax.rax_6:=handR_rotate.robax.rax_6+90;
        AccSet 100,100,\FinePointRamp:=100;
        MoveAbsJ handR_rotate,v800,z5,tGripper;
    ENDPROC

    PROC p_rotate_180_sub_R()
        handR_rotate:=CJointT();
        handR_rotate.robax.rax_6:=handR_rotate.robax.rax_6-180;
        AccSet 100,100,\FinePointRamp:=100;
        MoveAbsJ handR_rotate,v800,z5,tGripper;
    ENDPROC

    PROC p_rotate_90_sub_R()
        handR_rotate:=CJointT();
        handR_rotate.robax.rax_6:=handR_rotate.robax.rax_6-90;
        AccSet 100,100,\FinePointRamp:=100;
        MoveAbsJ handR_rotate,v800,z5,tGripper;
    ENDPROC

    PROC p_rotate_65_add_R()
        handR_rotate:=CJointT();
        handR_rotate.robax.rax_6:=handR_rotate.robax.rax_6+65;
        AccSet 100,100,\FinePointRamp:=100;
        MoveAbsJ handR_rotate,v800,z5,tGripper;
    ENDPROC

    PROC p_rotate_65_sub_R()
        handR_rotate:=CJointT();
        handR_rotate.robax.rax_6:=handR_rotate.robax.rax_6-65;
        AccSet 100,100,\FinePointRamp:=100;
        MoveAbsJ handR_rotate,v800,z5,tGripper;
    ENDPROC

    PROC p_rotate_205_add_R()
        handR_rotate:=CJointT();
        handR_rotate.robax.rax_6:=handR_rotate.robax.rax_6+205;
        AccSet 100,100,\FinePointRamp:=100;
        MoveAbsJ handR_rotate,v800,z5,tGripper;
    ENDPROC

    PROC p_rotate_205_sub_R()
        handR_rotate:=CJointT();
        handR_rotate.robax.rax_6:=handR_rotate.robax.rax_6-205;
        AccSet 100,100,\FinePointRamp:=100;
        MoveAbsJ handR_rotate,v800,z5,tGripper;
    ENDPROC
ENDMODULE